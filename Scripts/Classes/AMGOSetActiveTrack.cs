using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Holoville.HOTween;

[AddComponentMenu("")]
public class AMGOSetActiveTrack : AMTrack {
    [SerializeField]
    GameObject obj;

    public bool startActive = true;

	protected override void SetSerializeObject(UnityEngine.Object obj) {
		this.obj = obj as GameObject;
	}
	
	protected override UnityEngine.Object GetSerializeObject(GameObject targetGO) {
		return targetGO ? targetGO : obj;
	}

    public override string getTrackType() {
        return "GOSetActive";
    }
    // update cache
    public override void updateCache(AMITarget target) {
		base.updateCache(target);

        // add all clips to list
        for(int i = 0; i < keys.Count; i++) {
            AMGOSetActiveKey key = keys[i] as AMGOSetActiveKey;

            key.version = version;

            if(keys.Count > (i + 1)) key.endFrame = keys[i + 1].frame;
            else key.endFrame = -1;
        }
    }
    // preview a frame in the scene view
	public override void previewFrame(AMITarget target, float frame, AMTrack extraTrack = null) {
		GameObject go = GetTarget(target) as GameObject;

        if(keys == null || keys.Count <= 0) {
            return;
        }
		if(!go) return;

        // if before the first frame
        if(frame < (float)keys[0].frame) {
			//go.rotation = (cache[0] as AMPropertyAction).getStartQuaternion();
			go.SetActive(startActive);
            return;
        }
        // if beyond or equal to last frame
        if(frame >= (float)(keys[keys.Count - 1] as AMGOSetActiveKey).frame) {
			go.SetActive((keys[keys.Count - 1] as AMGOSetActiveKey).setActive);
            return;
        }
        // if lies on property action
        foreach(AMGOSetActiveKey key in keys) {
			if((frame < (float)key.frame) || (key.endFrame != -1 && frame >= (float)key.endFrame)) continue;

			go.SetActive(key.setActive);
            return;
        }
    }

	public override void buildSequenceStart(AMITarget target, Sequence s, int frameRate) {
		GameObject go = GetTarget(target) as GameObject;

        //need to add activate game object on start to 'reset' properly during reverse
        if(keys.Count > 0 && keys[0].frame > 0) {
			s.Insert(0.0f, HOTween.To(go, ((float)keys[0].frame) / ((float)frameRate),
			                          new TweenParms().Prop("active", new AMPlugGOActive(go, startActive))));
        }
    }

    // add a new key
	public AMKey addKey(AMITarget target, int _frame) {
        foreach(AMGOSetActiveKey key in keys) {
            // if key exists on frame, update
            if(key.frame == _frame) {
                key.setActive = true;
				updateCache(target);
                return null;
            }
        }
        AMGOSetActiveKey a = gameObject.AddComponent<AMGOSetActiveKey>();
        a.frame = _frame;
        a.setActive = true;
        // add a new key
        keys.Add(a);
        // update cache
		updateCache(target);
        return a;
    }

	public override AnimatorTimeline.JSONInit getJSONInit(AMITarget target) {
        // no initial values to set
        return null;
    }

	public override List<GameObject> getDependencies(AMITarget target) {
		GameObject go = GetTarget(target) as GameObject;
        List<GameObject> ls = new List<GameObject>();
		if(go) ls.Add(go);
        return ls;
    }

	public override List<GameObject> updateDependencies(AMITarget target, List<GameObject> newReferences, List<GameObject> oldReferences) {
		GameObject go = GetTarget(target) as GameObject;
        bool didUpdateObj = false;
		if(go) {
            for(int i = 0; i < oldReferences.Count; i++) {
				if(oldReferences[i] == go) {
					SetTarget(target, newReferences[i]);
                    didUpdateObj = true;
                    break;
                }

            }
        }
        if(didUpdateObj) updateCache(target);

        return new List<GameObject>();
    }

	protected override AMTrack doDuplicate(GameObject holder) {
        AMGOSetActiveTrack ntrack = holder.AddComponent<AMGOSetActiveTrack>();
        ntrack.enabled = false;
        ntrack.obj = obj;
        ntrack.startActive = startActive;

        return ntrack;
    }
}
