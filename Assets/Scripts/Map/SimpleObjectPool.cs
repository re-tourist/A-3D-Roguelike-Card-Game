using System.Collections.Generic;
using UnityEngine;

namespace Game.Map
{
    // 简易对象池（当前版本未在MapController中启用，可按需替换 Instantiate/Destroy）
    public class SimpleObjectPool<T> where T : Component
    {
        private readonly T prefab;
        private readonly Transform parent;
        private readonly Stack<T> stack = new Stack<T>();

        public SimpleObjectPool(T prefab, Transform parent)
        {
            this.prefab = prefab;
            this.parent = parent;
        }

        public T Get()
        {
            if (stack.Count > 0)
            {
                var item = stack.Pop();
                item.gameObject.SetActive(true);
                return item;
            }
            return Object.Instantiate(prefab, parent);
        }

        public void Release(T item)
        {
            item.gameObject.SetActive(false);
            stack.Push(item);
        }
    }
}