using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;

//Stolen from http://stackoverflow.com/a/3719378
public class LRUCache<K,V> {
	private int m_capacity;
	private Dictionary<K, LinkedListNode<LRUCacheItem<K, V>>> m_cacheMap = new Dictionary<K, LinkedListNode<LRUCacheItem<K, V>>>();
	private LinkedList<LRUCacheItem<K, V>> m_lruList = new LinkedList<LRUCacheItem<K, V>>();
	
	public LRUCache(int capacity = 8) { // TODO: Experiment with different defaults
		m_capacity = capacity;
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public V Get(K key) {
		LinkedListNode<LRUCacheItem<K, V>> node;
		if (m_cacheMap.TryGetValue(key, out node)) {
			V value = node.Value.value;
			
			m_lruList.Remove(node);
			m_lruList.AddLast(node);
			return value;
		}
		return default(V);
	}
	
	[MethodImpl(MethodImplOptions.Synchronized)]
	public void Add(K key, V val) {
		if (m_cacheMap.Count >= m_capacity) {
			RemoveFirst();
		}
		LRUCacheItem<K, V> cacheItem = new LRUCacheItem<K, V>(key, val);
		LinkedListNode<LRUCacheItem<K, V>> node = new LinkedListNode<LRUCacheItem<K, V>>(cacheItem);
		m_lruList.AddLast(node);
		m_cacheMap.Add(key, node);
	}

	public V this[K key] {
		get {
			return Get(key);
		}
	}
	
	
	protected void RemoveFirst() {
		// Remove from LRUPriority
		LinkedListNode<LRUCacheItem<K,V>> node = m_lruList.First;
		m_lruList.RemoveFirst();
		// Remove from cache
		m_cacheMap.Remove(node.Value.key);
	}
}


internal class LRUCacheItem<K,V> {
	public LRUCacheItem(K k, V v) {
		key = k;
		value = v;
	}
	public K key;
	public V value;
}