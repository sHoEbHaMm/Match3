using UnityEditor.PackageManager;
using UnityEngine;

/* SINGLETON PATTERN
 * 1. Whatever inherits from this class, will only allow one instance of itself to exists
 * while providing a static reference to it for easy access
 * 2. This class is a generic type with a constraint where T should be of type Singleton
 */
public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T instance;

    //Getter
    public static T Instance
    {
        get
        {
            if(instance == null)
            {
                Debug.LogError("No instance of " + typeof(T) + " exists in the scene");
            }

            return instance;
        }
    }

    //create the reference in Awake
    protected void Awake()
    {
        if(instance == null)
        {
            instance = this as T;
            Initialize();
        }
        else
        {
            Debug.LogWarning("An instance of " + typeof(T) + " already exists in the scene. Self-destructing");
            Destroy(this.gameObject);
        }
    }

    //destroy reference OnDestroy
    protected void OnDestroy()
    {
        if(this == instance)
        {
            instance = null;
        }
    }

    // Virtual function to initialize different inherited types
    protected virtual void Initialize()
    {

    }
}
