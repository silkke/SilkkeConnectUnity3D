using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Silkke
{
    public class DownloadManager : MonoBehaviour
    {
        public delegate void DLCallback(WWW www);

        private class DLHandler
        {
            public IEnumerator coroutine;
            public DLCallback onSuccess;
            public DLCallback onFailure;
            public DLCallback onUpdate;
            public WWW www;

            public DLHandler(DLCallback success, DLCallback failure, DLCallback update)
            {
                coroutine = null;
                onSuccess = success;
                onFailure = failure;
                onUpdate = update;
                www = null;
            }
        }

        static private Dictionary<string, DLHandler> workers;

        static private DownloadManager m_Instance;

        private static DownloadManager Instance
        {
            get
            {
                if (m_Instance == null)
                    m_Instance = (new GameObject("[Download Manager]")).AddComponent<DownloadManager>();
                return m_Instance;
            }
        }

        public void Awake()
        {
            if (m_Instance == null)
                m_Instance = this;
            else
            { Destroy(gameObject); return; }
            DontDestroyOnLoad(this);

            workers = new Dictionary<string, DLHandler>();
        }

        static public void Request(string url, DLCallback onSuccess, DLCallback onFailure, DLCallback onUpdate)
        {
            m_Instance._Request(url, onSuccess, onFailure, onUpdate);
        }

        static public void Request(string url, DLCallback onSuccess, DLCallback onFailure, ProgressBar progressBar)
        {
            DownloadManager.Request(url, onSuccess, onFailure, (www) =>
            {
                progressBar.setProgress(www.progress);
            });
        }

        static public void Cancel(string url)
        {
            m_Instance._Cancel(url);
        }

        static public void CancelAll()
        {
            m_Instance._CancelAll();
        }

        private void _Request(string url, DLCallback onSuccess, DLCallback onFailure, DLCallback onUpdate)
        {
            onUpdate = onUpdate ?? OnUpdate;
            _AddOrUpdateWorker(url, onSuccess, onFailure, onUpdate);
        }

        private void _Cancel(string url)
        {
            DLHandler worker;

            if (workers.TryGetValue(url, out worker) == true)
            {
                _CancelWorker(worker);
                workers.Remove(url);
            }
        }

        private void _CancelAll()
        {
            foreach (var worker in workers)
                _CancelWorker(worker.Value);
            workers.Clear();
        }

        private void _AddOrUpdateWorker(
            string url
            , DLCallback onSuccess
            , DLCallback onFailure
            , DLCallback onUpdate)
        {

            DLHandler worker;

            if (workers.TryGetValue(url, out worker) == false)
            {
                worker = new DLHandler(onSuccess, onFailure, onUpdate);

                // In case of bundle download of .unity3d file - Put in cache
                if (url.IndexOf("unity") > 0 && Application.platform != RuntimePlatform.WebGLPlayer)
                    worker.coroutine = RequestAndCache(url, worker);
                else
                    // In case of normal download - no caching
                    worker.coroutine = RequestCoroutine(url, worker);
                workers.Add(url, worker);
                StartCoroutine(worker.coroutine);
            }
            else
            {
                worker.onSuccess += onSuccess;
                worker.onFailure += onFailure;
                worker.onUpdate += onUpdate;
            }
        }

        private void _CancelWorker(DLHandler worker)
        {
            worker.www.Dispose();
            StopCoroutine(worker.coroutine);
        }

        private IEnumerator RequestCoroutine(string url, DLHandler worker)
        {

            using (worker.www = new WWW(url))
            {
                while (!worker.www.isDone)
                {
                    worker.onUpdate(worker.www);
                    yield return null;
                }
                worker.onUpdate(worker.www);
                workers.Remove(url);
                if (string.IsNullOrEmpty(worker.www.error))
                    worker.onSuccess(worker.www);
                else
                    worker.onFailure(worker.www);
            }
        }

        private IEnumerator RequestAndCache(string url, DLHandler worker)
        {
            while (!Caching.ready)
                yield return null;

            using (worker.www = WWW.LoadFromCacheOrDownload(url, Session.avatarID))
            {
                while (!worker.www.isDone)
                {
                    worker.onUpdate(worker.www);
                    yield return null;
                }

                workers.Remove(url);
                if (string.IsNullOrEmpty(worker.www.error))
                    worker.onSuccess(worker.www);
                else
                    worker.onFailure(worker.www);
            }
        }

        static public void OnUpdate(WWW www)
        {
            return;
        }
    }
}