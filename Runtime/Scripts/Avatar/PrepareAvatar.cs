using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GLTFast.Avaturn;
using UnityEngine;

namespace Avaturn
{
    [RequireComponent(typeof(Animator))]
    public class PrepareAvatar : MonoBehaviour
    {
        //Receives a downloaded model and converts it into mecanim avatar.
        [SerializeField] private DownloadAvatarEvents events;
        [Tooltip("Clear root gameObject out of prev avatar gameObjects. Start from that child index")]
        [SerializeField] private int _clearRootFromIndex;
        [SerializeField] private bool _worldPositionStaysForNewAvatar = true;
        
        private Animator _animator;

        private void Start()
        {
            try
            {
                events.OnSuccess += PrepareModel;
                _animator = GetComponent<Animator>();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in Start: {ex.Message}");
            }
        }

        private void OnDestroy()
        {
            try
            {
                events.OnSuccess -= PrepareModel;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in OnDestroy: {ex.Message}");
            }
        }

        private async void PrepareModel(Transform downloadedModel)
        {
            try
            {
                if (_animator.applyRootMotion)
                {
                    transform.localPosition = Vector3.zero;
                    transform.localRotation = Quaternion.identity;
                }

                downloadedModel.gameObject.SetActive(false);

                //delete prev model and skeleton
                for (int i = _clearRootFromIndex; i < transform.childCount; i++)
                {
                    Destroy(transform.GetChild(i).gameObject);
                }

                await Task.Yield();

                // Go through hierarchy and move model from downloaded avatar to base
                var root = downloadedModel.transform.GetChild(0);

                if (!root)
                {
                    Debug.LogWarning("Prepare failed. Can't find root object");
                    return;
                }

                // Move model from downloaded avatar to base
                var childCount = root.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    var child = root.GetChild(0);
                    child.SetParent(transform, _worldPositionStaysForNewAvatar);
                }

                _animator.avatar = HumanoidAvatarBuilder.Build(gameObject);

                await Task.Yield();
                Destroy(root.gameObject);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in PrepareModel: {ex.Message}");
            }
        }
    }
}
