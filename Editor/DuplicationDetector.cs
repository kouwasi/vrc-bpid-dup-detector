#if UNITY_EDITOR

using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.Core;
using VRC.SDKBase.Editor.BuildPipeline;

namespace VRCBPIDDuplicationDetector
{
  public class DuplicationDetector : IVRCSDKPreprocessAvatarCallback
  {
    public int callbackOrder => 0;

    public bool OnPreprocessAvatar(GameObject avatarGameObject)
    {
      PipelineManager pipelineManager = avatarGameObject.GetComponent<PipelineManager>();
      string blueprintId = pipelineManager.blueprintId;

      if (blueprintId == null || blueprintId.Length == 0)
      {
        Logger.Log("No Blueprint ID found. Skipping duplication check.");
        return true;
      }

      Logger.Log("Starting duplication check.");

      // たぶんビルド時に対象のアバターをクローンしてるので引数で渡されたオブジェクトは除外する
      if (IsDuplicated(blueprintId, pipelineManager.gameObject))
      {
        Logger.Log($"Duplicate Blueprint ID '{blueprintId}' found.");

        string confirmationMessage = $"Blueprint ID '{blueprintId}' が以下のアバターと重複しています。\n\n"
          + string.Join("\n", GetObjectNamesByBlueprintIds(new string[] { blueprintId }, pipelineManager.gameObject));

        if (!DisplayConfirmationDialog(confirmationMessage))
        {
          Logger.Log("Build aborted.");
          return false;
        }
      }

      Logger.Log($"No duplicates found for Blueprint ID '{blueprintId}'.");

      return true;
    }

    private static bool IsDuplicated(string blueprintId, GameObject excludeObject = null)
    {
      PipelineManager[] pipelineManagers = Object.FindObjectsByType<PipelineManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);

      int duplicateCount = pipelineManagers
        .Where(pm => pm.gameObject != excludeObject)
        .GroupBy(pm => pm.blueprintId)
        .Where(g => g.Key == blueprintId)
        .Select(g => g.Count())
        .First();

      return duplicateCount > 1;
    }

    private static bool DisplayConfirmationDialog(string message)
    {
      return EditorUtility.DisplayDialog(
        "Blueprint IDの重複を検知しました",
        message + "\n\nビルドを続行すると、意図せずアップロード済みのアバターを上書きしてしまう可能性があります。\nビルドを続行しますか？",
        "続行",
        "キャンセル"
      );
    }

    private static string[] GetObjectNamesByBlueprintIds(string[] blueprintIds, GameObject excludeObject = null)
    {
      PipelineManager[] pipelineManagers = Object.FindObjectsByType<PipelineManager>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);

      return pipelineManagers
        .Where(pm => blueprintIds.Contains(pm.blueprintId) && pm.gameObject != excludeObject)
        .Select(pm => pm.gameObject.name)
        .ToArray();
    }
  }
}

#endif
