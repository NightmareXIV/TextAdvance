using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TextAdvance
{
    unsafe class ClickManager
    {
        TextAdvance p;
        DalamudPluginInterface pi;
        const int ThrottleTime = 500;

        public ClickManager(TextAdvance p)
        {
            this.p = p;
            this.pi = p.pi;
        }
        protected ReceiveEventDelegate GetReceiveEventDelegate(AtkEventListener* eventListener)
        {
            var receiveEventAddress = new IntPtr(eventListener->vfunc[2]);
            return Marshal.GetDelegateForFunctionPointer<ReceiveEventDelegate>(receiveEventAddress);
        }

        int NextClick = 0;
        internal void SendClickThrottled(IntPtr arg1, EventType arg2, uint arg3, void* target)
        {
            if (Environment.TickCount > NextClick)
            {
                SendClick(arg1, arg2, arg3, target, IntPtr.Zero);
                NextClick = Environment.TickCount + ThrottleTime;
                pi.Framework.Gui.Chat.Print("Click Sent");
            }
            else
            {
                pi.Framework.Gui.Chat.Print("Click Throttled");
            }
        }
        internal void SendClickThrottled(IntPtr arg1, EventType arg2, uint arg3, void* target, IntPtr arg5)
        {
            if (Environment.TickCount > NextClick)
            {
                SendClick(arg1, arg2, arg3, target, arg5);
                NextClick = Environment.TickCount + ThrottleTime;
                pi.Framework.Gui.Chat.Print("Click Sent 2");
            }
            else
            {
                pi.Framework.Gui.Chat.Print("Click Throttled 2");
            }
        }

        internal void SendClick(IntPtr arg1, EventType arg2, uint arg3, void* target)
        {
            SendClick(arg1, arg2, arg3, target, IntPtr.Zero);
        }

        internal void SendClick(IntPtr arg1, EventType arg2, uint arg3, void* target, IntPtr arg5)
        {
            var receiveEvent = GetReceiveEventDelegate((AtkEventListener*)arg1);

            var arg4 = Marshal.AllocHGlobal(0x40);
            for (var i = 0; i < 0x40; i++)
                Marshal.WriteByte(arg4, i, 0);

            Marshal.WriteIntPtr(arg4, 0x8, new IntPtr(target));
            Marshal.WriteIntPtr(arg4, 0x10, arg1);

            if (arg5 == IntPtr.Zero)
            {
                arg5 = Marshal.AllocHGlobal(0x40);
                for (var i = 0; i < 0x40; i++)
                    Marshal.WriteByte(arg5, i, 0);
            }

            receiveEvent(arg1, arg2, arg3, arg4, arg5);

            Marshal.FreeHGlobal(arg4);
            Marshal.FreeHGlobal(arg5);
        }

        internal unsafe void SelectStringClick(IntPtr addonPtr, int index)
        {
            var addon = (AddonSelectString*)addonPtr;
            var popupMenu = &addon->PopupMenu;
            var componentList = popupMenu->List;

            var arg5 = Marshal.AllocHGlobal(0x40);
            for (var i = 0; i < 0x40; i++)
                Marshal.WriteByte(arg5, i, 0);

            Marshal.WriteIntPtr(arg5, new IntPtr(componentList->ItemRendererList[index].AtkComponentListItemRenderer));
            Marshal.WriteInt16(arg5, 0x10, (short)index);
            Marshal.WriteInt16(arg5, 0x16, (short)index);

            SendClickThrottled(new IntPtr(popupMenu), EventType.LIST_INDEX_CHANGE, 0, componentList->AtkComponentBase.OwnerNode, arg5);
        }

        protected delegate void ReceiveEventDelegate(IntPtr addon, EventType evt, uint a3, IntPtr a4, IntPtr a5);

        public enum EventType : ushort
        {
            NORMAL = 1,
            NORMAL_MAX = 2,
            MOUSE_DOWN = 3,
            MOUSE_UP = 4,
            MOUSE_MOVE = 5,
            MOUSE_ROLL_OVER = 6,
            MOUSE_ROLL_OUT = 7,
            MOUSE_WHEEL = 8,
            MOUSE_CLICK = 9,
            MOUSE_DOUBLE_CLICK = 10,
            MOUSE_MAX = 11,
            INPUT = 12,
            INPUT_KEY = 13,
            INPUT_MAX = 14,
            PAD = 15,
            PAD_MAX = 16,
            FOCUS_IN = 17,
            FOCUS_OUT = 18,
            FOCUS_MAX = 19,
            RESIZE = 20,
            RESIZE_MAX = 21,
            BUTTON_PRESS = 22,
            BUTTON_CLICK = 23,
            BUTTON_MAX = 24,
            CHANGE = 25,
            CHANGE_MAX = 26,
            SLIDER_CHANGE = 27,
            SLIDER_CHANGE_END = 28,
            LIST_ITEM_PRESS = 29,
            LIST_ITEM_UP = 30,
            LIST_ITEM_ROLL_OVER = 31,
            LIST_ITEM_ROLL_OUT = 32,
            LIST_ITEM_CLICK = 33,
            LIST_ITEM_DOUBLE_CLICK = 34,
            LIST_INDEX_CHANGE = 35,
            LIST_FOCUS_CHANGE = 36,
            LIST_ITEM_CANCEL = 37,
            LIST_ITEM_PICKUP_START = 38,
            LIST_ITEM_PICKUP_END = 39,
            LIST_ITEM_EXCHANGE = 40,
            LIST_TREE_EXPAND = 41,
            LIST_MAX = 42,
            DDL_LIST_OPEN = 43,
            DDL_LIST_CLOSE = 44,
            DD_DRAG_START = 45,
            DD_DRAG_END = 46,
            DD_DROP = 47,
            DD_DROP_EXCHANGE = 48,
            DD_DROP_NOTICE = 49,
            DD_ROLL_OVER = 50,
            DD_ROLL_OUT = 51,
            DD_DROP_STAGE = 52,
            DD_EXECUTE = 53,
            ICON_TEXT_ROLL_OVER = 54,
            ICON_TEXT_ROLL_OUT = 55,
            ICON_TEXT_CLICK = 56,
            DIALOGUE_CLOSE = 57,
            DIALOGUE_SUBMIT = 58,
            TIMER = 59,
            TIMER_COMPLETE = 60,
            SIMPLETWEEN_UPDATE = 61,
            SIMPLETWEEN_COMPLETE = 62,
            SETUP_ADDON = 63,
            UNIT_BASE_OVER = 64,
            UNIT_BASE_OUT = 65,
            UNIT_SCALE_CHANEGED = 66,
            UNIT_RESOLUTION_SCALE_CHANEGED = 67,
            TIMELINE_STATECHANGE = 68,
            WORDLINK_CLICK = 69,
            WORDLINK_ROLL_OVER = 70,
            WORDLINK_ROLL_OUT = 71,
            CHANGE_TEXT = 72,
            COMPONENT_IN = 73,
            COMPONENT_OUT = 74,
            COMPONENT_SCROLL = 75,
            COMPONENT_FOCUSED = 76 // Maybe
        }
    }
}
