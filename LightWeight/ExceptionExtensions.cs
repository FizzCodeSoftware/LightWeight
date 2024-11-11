namespace FizzCode;

public static class ExceptionExtensions
{
    public static string Format(this Exception exception, bool includeTrace = true, int skipStackFrames = 0)
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

                if (cex.Data?.Count > 0)
                {
                    var first = true;
                    var maxKeyLength = 0;
                    foreach (var key in cex.Data.Keys)
                    {
                        var l = key.ToString().Length;
                        if (l > maxKeyLength)
                            maxKeyLength = l;
                    }

                    foreach (var key in cex.Data.Keys)
                    {
                        var k = key.ToString();
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
                        msg += "\n\t\t" + ("[" + k + "]").PadRight(maxKeyLength + 3) + " = " + (value != null ? value.ToString().Trim() : "NULL");
                    }
                }

                if (includeTrace)
                {
                    if (cex.Data?["Trace"] is not string trace)
                        trace = GetTraceFromStackFrames(new StackTrace(cex, true).GetFrames(), lvl == 0 ? 1 + skipStackFrames : 0);

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

    public static string GetTraceFromStackFrames(StackFrame[] frames, int skipStackFrames = 0)
    {
        if (frames == null || frames.Length == 0)
            return null;

        var firstConstructorFiltered = false;
        var builder = new StringBuilder();

        var actualFrames = new List<(StackFrame Frame, MethodBase Method, string AssemblyName)>();
        foreach (var frame in frames.Skip(skipStackFrames))
        {
            var method = frame.GetMethod();
            if (method == null)
                continue;

            var assemblyName = method.DeclaringType?.Assembly?.GetName().Name;

            if (!firstConstructorFiltered)
            {
                if (frame.GetMethod().IsConstructor || frame.GetMethod().IsStatic)
                    continue;

                firstConstructorFiltered = true;
            }

            if (actualFrames.Count > 0)
            {
                if (assemblyName != null)
                {
                    if (assemblyName.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase))
                        continue;

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
            }

            actualFrames.Add((frame, method, assemblyName));
        }

        var maxAssemblyNameLength = actualFrames.Max(frame => frame.AssemblyName.Length);
        foreach (var (frame, method, assemblyName) in actualFrames)
            builder.AppendLine(FrameToString(frame, method, assemblyName, maxAssemblyNameLength));

        return builder.ToString().Trim();
    }

    private static string FrameToString(StackFrame frame, MethodBase method, string assemblyName, int maxAssemblyNameLength)
    {
        var sb = new StringBuilder();

        var ignoreMethod = false;

        if (assemblyName != null)
        {
            sb.Append("in ").Append(assemblyName.PadRight(maxAssemblyNameLength + 1)).Append(": ");
        }
        else
        {
            sb.Append("   ").Append("".PadRight(maxAssemblyNameLength + 1)).Append("  ");
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
