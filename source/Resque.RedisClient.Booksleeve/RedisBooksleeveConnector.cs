using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookSleeve;

namespace Resque.RedisClient.Booksleeve
{
    public class RedisBooksleeveConnector :IRedis
    {
        public string RedisNamespace { get; set; }
        public RedisConnection Client { get; set; }
        public int RedisDb { get; set; }

        public RedisBooksleeveConnector(RedisConnection client, int redisDb = 0, string redisNamespace = "resque")
        {
            Client = client;
            RedisDb = redisDb;
            RedisNamespace = redisNamespace;
        }
        public string KeyInNamespace(string key)
        {
            return string.Join(":", RedisNamespace, key);
        }
        public string[] KeyInNamespace(params string[] keys)
        {
            return keys.Select(x => string.Join(":", RedisNamespace, x)).ToArray();
        }

        public bool SAdd(string key, string redisId)
        {
            return Wait(Client.Sets.Add(RedisDb, KeyInNamespace(key), redisId));
        }

        public string LPop(string key)
        {
            return Wait(Client.Lists.RemoveFirstString(RedisDb, KeyInNamespace(key)));
        }

        public T Wait<T>(Task<T> task)
        {
            task.Wait();
            return task.Result;
        }

        public Tuple<string,string> BLPop(string[] keys, int timeoutSeconds)
        {
            try
            {
                return Wait(Client.Lists.BlockingRemoveFirstString(RedisDb, KeyInNamespace(keys), timeoutSeconds));
            }
            catch(TimeoutException)
            {
                return null;
            }
        }

        public Dictionary<string, string> HGetAll(string key)
        {
            return Wait(Client.Hashes.GetAll(RedisDb, KeyInNamespace(key))).ToDictionary(k=>k.Key, v=>FromUtf8Bytes(v.Value));
        }

        public string HGet(string key, string field)
        {
            return Wait(Client.Hashes.GetString(RedisDb, KeyInNamespace(key), field));
        }

        public bool HSet(string key, string field, string value)
        {
            return Wait(Client.Hashes.Set(RedisDb, KeyInNamespace(key), field, value));
        }

        public bool ZAdd(string key, string value, long score)
        {
            return Wait(Client.SortedSets.Add(RedisDb, KeyInNamespace(key), value, score));
        }

        public long ZCard(string key)
        {
            return Wait(Client.SortedSets.GetLength(RedisDb, KeyInNamespace(key)));
        }

        public long ZCard(string key, long min, long max)
        {
            return Wait(Client.SortedSets.GetLength(RedisDb, KeyInNamespace(key), min, max));
        }

        public Tuple<string, double>[] ZRange(string key, long start, long stop, bool ascending = false)
        {
            return Wait(Client.SortedSets.RangeString(RedisDb, KeyInNamespace(key), start, stop, ascending))
                .Select(x=>new Tuple<string, double>(x.Key, x.Value))
                .ToArray();
        }
        public double ZScore(string key, string member)
        {
            return Wait(Client.SortedSets.Score(RedisDb, KeyInNamespace(key), member));
        }

        private static string FromUtf8Bytes(byte[] bytes)
		{
			return bytes == null ? null : Encoding.UTF8.GetString(bytes);
		}
        
        public long Incr(string key)
        {
            return Wait(Client.Strings.Increment(RedisDb, KeyInNamespace(key)));
        }

        public IEnumerable<string> SMembers(string key)
        {
            return Wait(Client.Sets.GetAllString(RedisDb, KeyInNamespace(key)));
        }

        public bool Exists(string key)
        {
            return Wait(Client.Keys.Exists(RedisDb, KeyInNamespace(key)));
        }

        public string Get(string key)
        {
            return Wait(Client.Strings.GetString(RedisDb, KeyInNamespace(key)));
        }

        public void Set(string key, string value)
        {
            Client.Strings.Set(RedisDb, KeyInNamespace(key), value).Wait();
        }

        public long RemoveKeys(params string[] keys)
        {
            return Wait(Client.Keys.Remove(RedisDb, KeyInNamespace(keys)));
        }

        public long SRemove(string key, params string[] values)
        {
            return Wait(Client.Sets.Remove(RedisDb, KeyInNamespace(key), values));
        }

        public long RPush(string key, string value)
        {
            return Wait(Client.Lists.AddLast(RedisDb, KeyInNamespace(key), value));
        }
    }
}