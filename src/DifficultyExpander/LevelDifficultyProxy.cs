using System;
using System.Reflection;
using System.Collections.Concurrent;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Messaging;

// Динамический прокси для интерфейсов IPreviewBeatmapLevel.
// Возвращает заданную difficultyName для методов/свойств, связанных со сложностью.
// Все остальные вызовы делегируются оригиналу.
public class LevelDifficultyProxy : RealProxy {
    object original;
    string difficultyName;

    static ConcurrentDictionary<object, string> proxyToDiff = new ConcurrentDictionary<object, string>();

    private LevelDifficultyProxy(Type type, object originalInstance, string difficulty) : base(type) {
        this.original = originalInstance;
        this.difficultyName = difficulty;
    }

    public static object Create(Type ifaceType, object originalInstance, string difficulty) {
        try {
            if (ifaceType == null || !ifaceType.IsInterface) return null;
            
            var proxy = new LevelDifficultyProxy(ifaceType, originalInstance, difficulty);
            var proxyObj = proxy.GetTransparentProxy();
            
            if (proxyObj != null) {
                proxyToDiff[proxyObj] = difficulty;
            }
            
            return proxyObj;
        } catch {
            return null;
        }
    }

    public override IMessage Invoke(IMessage msg) {
        try {
            var methodCall = msg as IMethodCallMessage;
            if (methodCall == null) return null;

            var method = methodCall.MethodBase as MethodInfo;
            if (method == null) return null;

            var name = method.Name.ToLowerInvariant();
            
            // Перехватываем вызовы, связанные со сложностью
            if (name.Contains("difficulty") || name.Contains("difficultyname")) {
                var retType = method.ReturnType;
                object result = null;
                
                if (retType == typeof(string)) {
                    result = difficultyName;
                } else if (retType.IsEnum) {
                    try { 
                        result = Enum.Parse(retType, difficultyName, true); 
                    } catch { 
                        result = Activator.CreateInstance(retType);
                    }
                } else if (retType == typeof(int)) {
                    result = 0;
                }
                
                if (result != null) {
                    return new ReturnMessage(result, null, 0, methodCall.LogicalCallContext, methodCall);
                }
            }

            // Делегируем вызов оригинальному объекту
            var realResult = method.Invoke(original, methodCall.Args);
            return new ReturnMessage(realResult, null, 0, methodCall.LogicalCallContext, methodCall);
            
        } catch (TargetInvocationException tie) {
            return new ReturnMessage(tie.InnerException ?? tie, msg as IMethodCallMessage);
        } catch (Exception ex) {
            return new ReturnMessage(ex, msg as IMethodCallMessage);
        }
    }

    public static string TryGetDifficultyName(object maybeProxy) {
        if (maybeProxy == null) return null;
        if (proxyToDiff.TryGetValue(maybeProxy, out var v)) return v;
        try {
            var t = maybeProxy.GetType();
            var f = t.GetField("difficultyName", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (f != null) {
                var val = f.GetValue(maybeProxy) as string;
                if (!string.IsNullOrEmpty(val)) return val;
            }
        } catch { }
        return null;
    }
}
