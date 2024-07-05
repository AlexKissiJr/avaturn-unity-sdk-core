using System;
using System.Threading.Tasks;
using GLTFast;
using GLTFast.Loading;
using UnityEngine;

namespace Avaturn
{
    /// <summary>
    /// This example of loading avatar model.
    /// </summary>
    [RequireComponent(typeof(GltfAsset))]
    public class DownloadAvatar : MonoBehaviour
    {
        [SerializeField] private DownloadAvatarEvents _events;

        [Tooltip("Use this for debug along with 'start url' to load avatar at runtime on start.")]
        [SerializeField] private bool _downloadOnStart;
        [SerializeField] private string _startUrl;

        private const string AvaturnStartUrlKey = "AvaturnStartUrl";
        private const string AvaturnDownloadOnStartKey = "HasAvaturnDownloadOnStart";

        private void Start()
        {
            // Load values from PlayerPrefs
            _downloadOnStart = PlayerPrefs.GetInt(AvaturnDownloadOnStartKey, 0) == 1;
            _startUrl = PlayerPrefs.GetString(AvaturnStartUrlKey, _startUrl);

            Debug.Log($"Loaded downloadOnStart: {_downloadOnStart}, startUrl: {_startUrl}");

            if (_downloadOnStart && !string.IsNullOrEmpty(_startUrl))
            {
                Download(_startUrl);
            }
        }

        public async void Download(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                Debug.LogError("Fail to download: url is empty");
                return;
            }
            Debug.Log($"Jesus: Start download...\nUrl = {url}");

            // Loading via GltFast loader
            var asset = GetComponent<GltfAsset>();
            asset.ClearScenes();
            var success = await asset.Load(url, new AvaturnDownloadProvider());

            // Optional for animations
            if (success)
            {
                _events.OnSuccess?.Invoke(transform);

                // Save the URL to PlayerPrefs only after successful download
                PlayerPrefs.SetString(AvaturnStartUrlKey, url);
                PlayerPrefs.SetInt(AvaturnDownloadOnStartKey, 1); // Set download on start to true
                PlayerPrefs.Save(); // Force save to disk

                Debug.Log($"URL saved: {url}");
            }
            else
            {
                Debug.LogError($"Fail to download");
                PlayerPrefs.SetInt(AvaturnDownloadOnStartKey, 0); // Reset download on start if failed
                PlayerPrefs.Save();
            }
        }

        public async Task<IDownload> Request(Uri url)
        {
            var req = new AvaturnAwaitableDownload(url);
            await req.WaitAsync();
            return req;
        }
    }
}
