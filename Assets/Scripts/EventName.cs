using System.IO;
using System;
using System.Collections.Generic;


public static class EventName
{
    static private Dictionary<int, string> map1 =
        new Dictionary<int, string> ();


    static private Dictionary<string, int> map2 =
        new Dictionary<string, int> ();


    public static void Install() {

        map1 [1] = "e_test";

        map2 ["e_test"] = 1;

    }

    public static string GetEventName(int cmd) {
        return map1[cmd];
    }

    public static int GetEventCmd(string name) {
        return map2 [name];
    }
};
