using System;
using System.Threading;

namespace HelpersLib
{
    public class ThreadWorker
    {
        public event Action DoWork;
        public event Action Completed;

        private SynchronizationContext context;
        private Thread thread;

        public ThreadWorker()
        {
            context = SynchronizationContext.Current ?? new SynchronizationContext();
        }

        public void Start(ApartmentState state = ApartmentState.MTA)
        {
            if (thread == null)
            {
                thread = new Thread(WorkThread);
                thread.IsBackground = true;
                thread.SetApartmentState(state);
                thread.Start();
            }
        }

        private void WorkThread()
        {
            OnDoWork();
            OnCompleted();
        }

        private void OnDoWork()
        {
            DoWork?.Invoke();
        }

        private void OnCompleted()
        {
            if (Completed != null)
            {
                InvokeAsync(Completed);
            }
        }

        public void Invoke(Action action)
        {
            context.Send(state => action(), null);
        }

        public void InvokeAsync(Action action)
        {
            context.Post(state => action(), null);
        }
    }
}
