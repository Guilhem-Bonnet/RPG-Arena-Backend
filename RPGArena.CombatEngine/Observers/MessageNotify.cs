using RPGArena.CombatEngine.Interface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGArena.CombatEngine.Observeur
{
    public class MessageNotify : IMessageNotifier
    {
        private readonly ConcurrentQueue<Message> _queue = new();
        private readonly List<IMessageObserver> _observers = new();

        public void RegisterObserver(IMessageObserver observer) => _observers.Add(observer);

        public void AddMessageToQueue(Message msg)
        {
            _queue.Enqueue(msg);
            ProcessQueue();
        }

        private void ProcessQueue()
        {
            while (_queue.TryDequeue(out var msg))
            {
                foreach (var obs in _observers)
                    obs.Update(msg);
            }
        }
    }

}
