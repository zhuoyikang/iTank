using System.Collections.Generic;
using System.Threading;
using System;
using UnityEngine;


/// <summary>
/// in:
/// 注册监听由渲染表现层抛出的事件(in = render->kbe)
/// 通常由kbe插件层来注册， 例如：UI层点击登录， 此时需要触发一个事件给kbe插件层进行与服务端交互的处理。
///
/// out:
/// 注册监听由kbe插件抛出的事件。(out = kbe->render)
/// 通常由渲染表现层来注册, 例如：监听角色血量属性的变化， 如果UI层注册这个事件，
/// 事件触发后就可以根据事件所附带的当前血量值来改变角色头顶的血条值。
/// </summary>

public static class NetEvent  {

    public struct Pair {
        public object _obj;
        public string _funcname;
        public System.Reflection.MethodInfo _method;
    }

    public struct EventObj {
        public Pair _info;
        public object[] _args;
    }

    public class HandleTable: Dictionary<string, List<Pair>>
    {
        public LinkedList<EventObj> _fired;
        public LinkedList<EventObj> _doing;

        public HandleTable() {
            _fired = new LinkedList<EventObj> ();
            _doing = new LinkedList<EventObj> ();
        }

        public void Lock() {
            Monitor.Enter(this);
        }

        public void UnLock() {
            Monitor.Exit(this);
        }

        public void ClearAll() {
            Lock();
            _fired.Clear();
            _doing.Clear();
            Clear();
            UnLock();
        }
    }


    public static HandleTable _handles_out = new HandleTable();
    public static HandleTable _handles_in = new HandleTable();


    public static void Clear()
    {
        _handles_in.Clear();
        _handles_out.Clear();
    }

    private static bool _hasRegister(HandleTable handles,
                                     string eventname) {
        bool has;

        handles.Lock();
        has = handles.ContainsKey(eventname);
        handles.UnLock();

        return has;
    }

    private static bool _register(HandleTable handles,
                                  string eventname,
                                  object obj,
                                  string funcname)
    {
        _unregister(handles, eventname, obj, funcname);
        List<Pair> lst;

        var pair = new Pair();
        pair._obj = obj;
        pair._funcname = funcname;
        pair._method = obj.GetType().GetMethod(funcname);

        if(pair._method == null) {
            Debug.Log("funcname not exist: "+ eventname + " " +funcname);
            return false;
        }

        handles.Lock();

        if(!handles.TryGetValue(eventname, out lst)) {
            lst = new List<Pair>();
            lst.Add(pair);
            handles.Add(eventname, lst);
            handles.UnLock();
            return true;
        }

        lst.Add(pair);
        handles.UnLock();

        return true;
    }

    private static bool _unregister(HandleTable handles,
                                    string eventname,
                                    object obj,
                                    string funcname)
    {
        handles.Lock();
        List<Pair> lst;

        if(!handles.TryGetValue(eventname, out lst))
        {
            handles.UnLock();
            return false;
        }

        for(int i=0; i<lst.Count; i++)
        {
            if(obj == lst[i]._obj && lst[i]._funcname == funcname)
            {
                lst.RemoveAt(i);
                handles.UnLock();
                return true;
            }
        }

        handles.UnLock();
        return false;
    }

    private static void _fire(HandleTable handles,
                              LinkedList<EventObj> firedEvents,
                              string eventname,
                              object[] args)
    {
        handles.Lock();
        List<Pair> lst;

        if(!handles.TryGetValue(eventname, out lst)){
            handles.UnLock();
            return;
        }

   //     Debug.Log("fire 1:"+ eventname+ " lst:" + lst.Count);

        for (int i = 0; i< lst.Count; i++) {
            var eobj = new EventObj();
            eobj._info = lst[i];
            eobj._args = args;
            firedEvents.AddLast(eobj);
        }

        handles.UnLock();
    }


    public static void _process(HandleTable handles) {

        handles.Lock();
        if(handles._fired.Count>0){
            foreach(EventObj evt in handles._fired) {
                handles._doing.AddLast(evt);
            }
            handles._fired.Clear();
        }
        handles.UnLock();

        while(handles._doing.Count>0){
            EventObj eobj = handles._doing.First.Value;
            //Debug.Log("process msg : "+ eobj._info._funcname);
            try {
                eobj._info._method.Invoke(eobj._info._obj, eobj._args);
            }
            catch(Exception e){
                Debug.Log("Event::processOutEvents: event=" +
                          eobj._info._funcname + "\n" + e);
            }
            handles._doing.RemoveFirst();
        }

    }

    public static void FireIn(string eventname, object[] args) {
        _fire(_handles_in, _handles_in._fired, eventname, args);
    }

    public static void FireOut(string eventname, object[] args) {
        _fire(_handles_out, _handles_out._fired, eventname, args);
    }

    public static void ProcessIn() {
        _process(_handles_in);
    }

    public static void ProcessOut() {
        _process(_handles_out);
    }


    public static bool RegisterIn(string eventname,
                                  object obj,
                                  string funcname) {
        return _register(_handles_in, eventname, obj, funcname);
    }

    public static bool UnRegisterIn(string eventname,
                                    object obj,
                                    string funcname) {
        return _unregister(_handles_in, eventname, obj, funcname);
    }

    public static bool RegisterOut(string eventname,
                                   object obj,
                                   string funcname) {
        return _register(_handles_out, eventname, obj, funcname);
    }

    public static bool UnRegisterOut(string eventname,
                                     object obj,
                                     string funcname) {
        return _unregister(_handles_out, eventname, obj, funcname);
    }



}
