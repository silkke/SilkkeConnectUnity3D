using Silkke;
using System;
using UnityEngine;
using System.Reflection;

public class avatarInitializer : MonoBehaviour
{
    [Header("Avatar Spawn Config.")]
    public Transform avatarSpawn;
    public bool useSpawnRotation = false;

    [Header("Avatar animation Config.")]
    public RuntimeAnimatorController avatarAnimator;
    public bool useRootMotion = false;

    [Header("Avatar Config.")]
    public Shader avatarShader;

    void Start()
    {
        Session.avatarInstance = BindAvatar(Session.InstantiateAvatar(), this.gameObject);
    }

    // 1er param : cible , 2eme param : source copy
    public Transform BindAvatar(Transform avatar, GameObject preset)
    {
        avatar.tag = "Player";
        avatar.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        Component[] components = preset.GetComponents<Component>();
        foreach (Component comp in components)
        {
            // if the newly added avatar doesn't have component => Add it
            if (!avatar.gameObject.GetComponent(comp.GetType()) &&
                !(comp is avatarInitializer) && !(comp is Rigidbody) && !(comp is AudioSource))
            {
                avatar.gameObject.AddComponent(comp.GetType());
                // Copy component value in our avatar
                instantiation.GetCopyOf(avatar.gameObject.GetComponent(comp.GetType()), comp);
            }
        }

        // Set material
        if (avatarShader && !Session.isDefaultAvatar)
            avatar.Find("Geometry").GetComponent<SkinnedMeshRenderer>().material.shader = avatarShader;

        // Set animator controller
        if (avatarAnimator)
        {
            avatar.GetComponent<Animator>().runtimeAnimatorController = avatarAnimator;
            avatar.GetComponent<Animator>().applyRootMotion = useRootMotion;
        }

        // Set avatar position and rotation
        if (avatarSpawn)
        {
            avatar.position = avatarSpawn.position;
            avatar.rotation = useSpawnRotation ? avatarSpawn.rotation : Quaternion.identity;
        }

        // Destroy avatar preset
        Destroy(preset);

        return avatar;
    }
}

public static class instantiation
{
    public static T GetCopyOf<T>(this Component comp, T other) where T : Component
    {
        Type type = comp.GetType();

        if (type != other.GetType()) return null;
        BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly | BindingFlags.ExactBinding;
        PropertyInfo[] pinfos = type.GetProperties(flags);

        foreach (var pinfo in pinfos)
        {
            if (pinfo.CanWrite)
            {
                try
                {
                    pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                }
                catch
                { }
            }
        }
        FieldInfo[] finfos = type.GetFields(flags);
        foreach (var finfo in finfos)
            finfo.SetValue(comp, finfo.GetValue(other));
        return comp as T;
    }
}