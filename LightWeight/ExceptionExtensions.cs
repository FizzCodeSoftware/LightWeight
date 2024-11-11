namespace FizzCode;

public static class ExceptionExtensions
{
    public static string FormatExceptionWithDetails(this Exception exception, bool includeTrace = true)
    {
        try
        {
            var lvl = 0;
            var msg = "EXCEPTION: ";

            var cex = exception;
            while (cex != null)
            {
                if (lvl > 0)
                    msg += "\nINNER EXCEPTION: ";

                msg += cex.GetType().GetFriendlyTypeName() + ": " + cex.Message;

                if (cex.Data?["ProcessType"] is string processType)
                {
                    msg += "\n\tPROCESS: ";

                    if (cex.Data?["ProcessTypeAssembly"] is string processTypeAssembly)
                        msg += "(" + processTypeAssembly + ") ";

                    msg += processType;

                    if (cex.Data?["ProcessName"] is string processName && processName != processType)
                        msg += " (\"" + processName + "\")";

                    if (cex.Data?["ProcessKind"] is string processKind)
                        msg += ", kind: " + processKind;
                }

                if (cex.Data?.Count > 0)
                {
                    var first = true;
                    foreach (var key in cex.Data.Keys)
                    {
                        var k = key.ToString();
                        if (k is "ProcessName" or "ProcessKind" or "ProcessType" or "ProcessTypeAssembly" or "CallChain" or "OpsMessage" or "Trace" or "Row")
                            continue;

                        if (k.Contains("Row", StringComparison.InvariantCultureIgnoreCase) && cex.Data[key] is string rowStr && rowStr.StartsWith("id", StringComparison.InvariantCultureIgnoreCase))
                            continue;

                        if (first)
                        {
                            msg += "\n\tDATA:";
                            first = false;
                        }
                        else
                        {
                            msg += ", ";
                        }

                        var value = cex.Data[key];
                        msg += "\n\t\t[" + k + "] = " + (value != null ? value.ToString().Trim() : "NULL");
                    }
                }

                if (cex.Data?["Row"] is string storedRow)
                {
                    msg += "\n\tROW: " + storedRow.Replace("\n", "\n\t\t", StringComparison.InvariantCultureIgnoreCase);
                }

                if (cex.Data?.Count > 0)
                {
                    foreach (var key in cex.Data.Keys)
                    {
                        var k = key.ToString();
                        if (k == "Row")
                            continue;

                        if (k.Contains("Row", StringComparison.InvariantCultureIgnoreCase) && cex.Data[key] is string rowStr && rowStr.StartsWith("id", StringComparison.InvariantCultureIgnoreCase))
                        {
                            msg += "\n\t" + k.ToUpperInvariant() + ": " + rowStr;
                        }
                    }
                }

                if (cex.Data?["CallChain"] is string callChain)
                    msg += "\n\tCALL CHAIN:\n\t\t" + callChain.Replace("\n", "\n\t\t", StringComparison.Ordinal);

                if (includeTrace)
                {
                    if (cex.Data?["Trace"] is not string trace)
                        trace = GetTraceFromStackFrames(new StackTrace(cex, true).GetFrames());

                    if (trace != null)
                    {
                        msg += "\n\tTRACE:\n\t\t" + trace.Replace("\n", "\n\t\t", StringComparison.Ordinal);
                    }
                }

                cex = cex.InnerException;
                lvl++;
            }

            return msg;
        }
        catch (Exception)
        {
            return exception.ToString();
        }
    }

    public static string GetTraceFromStackFrames(StackFrame[] frames)
    {
        if (frames == null || frames.Length == 0)
            return null;

        var ctorsFiltered = false;
        var currentFrameAdded = false;
        var builder = new StringBuilder();

        var maxAssemblyNameLength = frames.Max(frame =>
        {
            var method = frame.GetMethod();
            if (method == null)
                return 0;

            return (method.DeclaringType?.Assembly?.GetName().Name)?.Length ?? 0;
        });

        foreach (var frame in frames)
        {
            var method = frame.GetMethod();
            if (method == null)
                continue;

            var assemblyName = method.DeclaringType?.Assembly?.GetName().Name;

            if (!ctorsFiltered)
            {
                if (frame.GetMethod().IsConstructor || frame.GetMethod().IsStatic)
                    continue;

                ctorsFiltered = true;
            }

            if (!currentFrameAdded)
            {
                builder.AppendLine(FrameToString(frame, method, assemblyName, maxAssemblyNameLength));
                currentFrameAdded = true;
                continue;
            }

            if (assemblyName != null)
            {
                if (assemblyName.StartsWith("FizzCode.EtLast.CommandService", StringComparison.OrdinalIgnoreCase))
                    continue;

                try
                {
                    var fileName = frame.GetFileName();
                    if (string.IsNullOrEmpty(fileName))
                        continue;
                }
                catch (NotSupportedException)
                {
                    continue;
                }
                catch (SecurityException)
                {
                    continue;
                }
            }

            builder.AppendLine(FrameToString(frame, method, assemblyName, maxAssemblyNameLength));
        }

        return builder.ToString().Trim();
    }

    private static string FrameToString(StackFrame frame, MethodBase method, string assemblyName, int maxAssemblyNameLength)
    {
        var sb = new StringBuilder(200);

        var ignoreMethod = false;

        if (assemblyName != null)
        {
            sb.Append("in ").Append(assemblyName.PadRight(maxAssemblyNameLength + 1)).Append(": ");
        }

        if (!method.Name.StartsWith('<') && method.DeclaringType != null)
        {
            if (method.DeclaringType.Name.StartsWith('<'))
            {
                var endIndex = method.DeclaringType.Name.IndexOf('>', StringComparison.Ordinal);
                if (endIndex > -1 && endIndex < method.DeclaringType.Name.Length)
                {
                    switch (method.DeclaringType.Name[endIndex + 1])
                    {
                        case 'd':
                            sb.Append(TypeExtensions.FixGeneratedName(method.DeclaringType.DeclaringType.Name))
                                .Append('.');
                            ignoreMethod = true;
                            break;
                    }
                }
            }

            sb.Append(TypeExtensions.FixGeneratedName(method.DeclaringType.Name));
            if (!ignoreMethod)
                sb.Append('.');
        }

        if (!ignoreMethod)
        {
            sb.Append(TypeExtensions.FixGeneratedName(method.Name));

            if (method is MethodInfo mi && mi.IsGenericMethod)
            {
                sb.Append('<')
                    .AppendJoin(",", mi.GetGenericArguments().Select(x => x.GetFriendlyTypeName(false)))
                    .Append('>');
            }

            sb.Append('(')
                .AppendJoin(", ", method.GetParameters().Select(mp => mp.ParameterType.GetFriendlyTypeName() + " " + mp.Name))
                .Append(')');
        }

        try
        {
            var fileName = frame.GetFileName();
            if (frame.GetNativeOffset() != -1 && fileName != null)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, " in {0}, line {1}", Path.GetFileName(fileName), frame.GetFileLineNumber());
            }
        }
        catch (NotSupportedException)
        {
        }
        catch (SecurityException)
        {
        }

        return sb.ToString();
    }
}
