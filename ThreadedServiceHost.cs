using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Threading;
using System.Data;

namespace ServiceHostUtility
{
    class ThreadedServiceHost<TService, TContract>
    {
        const int SleepTime = 100;
        private ServiceHost m_serviceHost = null;
        private Thread m_thread;
        private string 
            m_serviceAddress,
            m_endpointAddress;
        private bool m_running;
        private Binding m_binding;

        public ThreadedServiceHost(string serviceAddress, string endpointAddress, Binding binding)
        {
            m_binding = binding;
            m_serviceAddress = serviceAddress;
            m_endpointAddress = endpointAddress;

            m_thread = new Thread(new ThreadStart(ThreadMethod));
            m_thread.Start();
        }

        void ThreadMethod()
        {
            try
            {
                m_running = true;

                // Start the host
                m_serviceHost = new ServiceHost(typeof(TService), new Uri(m_serviceAddress));
                m_serviceHost.AddServiceEndpoint(typeof(TContract), m_binding, m_endpointAddress);
                m_serviceHost.Open();

                while (m_running)
                {
                    // Wait until thread is stopped
                    Thread.Sleep(SleepTime);
                }

                m_serviceHost.Close();
            }
            catch (Exception)
            {
                if (m_serviceHost != null)
                {
                    m_serviceHost.Close();
                }
            }
        }

        /// <summary>
        /// Request the end of the thread method.
        /// </summary>
        public void Stop()
        {
            lock (this)
            {
                m_running = false;
            }
        }
    }
}
