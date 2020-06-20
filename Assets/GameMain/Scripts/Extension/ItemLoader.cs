﻿using GameFramework;
using GameFramework.Event;
using UnityGameFramework.Runtime;
using System;
using System.Collections.Generic;

namespace Flower
{
    public class ItemLoader : IReference
    {
        private Dictionary<int, Action<Item>> dicCallback;
        private Dictionary<int, Item> dicSerial2Item;

        public object Owner
        {
            get;
            private set;
        }

        public ItemLoader()
        {
            dicSerial2Item = new Dictionary<int, Item>();
            dicCallback = new Dictionary<int, Action<Item>>();
            Owner = null;

        }

        public int ShowItem(EnumItem enumItem, Action<Item> onShowSuccess, object userData = null)
        {
            int serialId = GameEntry.Item.GenerateSerialId();
            dicCallback.Add(serialId, onShowSuccess);
            GameEntry.Item.ShowItem(serialId, enumItem, Owner);
            return serialId;
        }

        public int ShowItem<T>(EnumItem enumItem, Action<Item> onShowSuccess, object userData = null) where T : ItemLogic
        {
            int serialId = GameEntry.Item.GenerateSerialId();
            dicCallback.Add(serialId, onShowSuccess);
            GameEntry.Item.ShowItem<T>(serialId, enumItem, Owner);
            return serialId;
        }

        public Item GetItem(int serialId)
        {
            if (dicSerial2Item.ContainsKey(serialId))
            {
                return dicSerial2Item[serialId];
            }

            return null;
        }

        public void HideItem(int serialId)
        {
            Item item = null;
            if (!dicSerial2Item.TryGetValue(serialId, out item))
            {
                Log.Error("Can find item('serial id:{0}') ", serialId);
            }

            dicSerial2Item.Remove(serialId);
            dicCallback.Remove(serialId);

            GameEntry.Item.HideItem(item);
        }

        public void HideItem(Item item)
        {
            if (item == null)
                return;

            GameEntry.Item.HideItem(item.Id);
        }

        public void HideAllItem()
        {
            foreach (var serialId in dicSerial2Item.Keys)
            {
                GameEntry.Item.HideItem(serialId);
            }

            dicSerial2Item.Clear();
            dicCallback.Clear();
        }

        private void OnShowItemSuccess(object sender, GameEventArgs e)
        {
            ShowItemSuccessEventArgs ne = (ShowItemSuccessEventArgs)e;
            if ((object)ne.UserData != Owner)
            {
                return;
            }

            Action<Item> callback = null;
            if (!dicCallback.TryGetValue(ne.Item.Id, out callback))
            {
                Log.Error("Show item callback is null,item('serial id:{0}') ", ne.Item.Id);
                return;
            }

            dicSerial2Item.Add(ne.Item.Id, ne.Item);

            callback(ne.Item);
        }

        private void OnShowItemFail(object sender, GameEventArgs e)
        {
            ShowItemFailureEventArgs ne = (ShowItemFailureEventArgs)e;
            if ((object)ne.UserData != Owner)
            {
                return;
            }

            Log.Warning("Show item failure with error message '{0}'.", ne.ErrorMessage);
        }

        public static ItemLoader Create(object owner)
        {
            ItemLoader itemLoader = ReferencePool.Acquire<ItemLoader>();
            itemLoader.Owner = owner;
            GameEntry.Event.Subscribe(ShowItemSuccessEventArgs.EventId, itemLoader.OnShowItemSuccess);
            GameEntry.Event.Subscribe(ShowItemFailureEventArgs.EventId, itemLoader.OnShowItemFail);

            return itemLoader;
        }

        public void Clear()
        {
            Owner = null;
            dicSerial2Item.Clear();
            dicCallback.Clear();
            GameEntry.Event.Unsubscribe(ShowItemSuccessEventArgs.EventId, OnShowItemSuccess);
            GameEntry.Event.Unsubscribe(ShowItemFailureEventArgs.EventId, OnShowItemFail);
        }
    }
}

