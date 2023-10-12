/**
 * This class is based on an original from Unity Technologies ApS - see below for the license - modified
 * slightly for our needs.
 *
 * If/when Unity releases an official version of the original class, this class will be deleted and we
 * will adopt the official version instead.
 *
 * Version: 2023.9.24
 * 
 * Changes from Version 2019:
 * 
 * Changes from Unity's original:
 * 1. Unity's original used compile-time flags, but when building DLLs or embedding DLLs these wont get rechecked later - this may not be a problem (this class is already an Editor-only class by definition?), but just in case: added the Application.isEditor logic
 * 2. Unity's original used the legacy "transform.parent = " which Unity staff have told me to avoid in the past (has bugs - especiall affects UI/uGUI - that Unity will probably never fix due to the compatibility issues it would cause)
 * 2b. ... which also means it had no support for the bool flag on Transform.SetParent(), which we use almost all the time
 * 3. Unity's original was missing an entry for "destroyObject"
 * 4. There appeared to be a single missing call to m_dirty (when reparenting) - all other methods were setting this
 * 5: added a 'DestroyGameObject' which Unity requires but the original class was missing
 * 6: added an auto-detect for Unity's embarrassingly badly-written Undo API
 */
/*
// Labs Utilities copyright © 2020 Unity Technologies ApS

// Licensed under the Unity Companion License for Unity-dependent projects--see Unity Companion License.

// Unless expressly provided otherwise, the Software under this license is made available strictly on an
// “AS IS” BASIS WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED. Please review the license for details
// on these and other terms and conditions.
*/
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityObject = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace PublishersFork
{
    /// <summary>
    /// Class that automatically groups a series of object actions together as a single undo-operation
    /// And works in both the editor and player (with player support simply turning off undo-operations)
    /// Mirrors the normal functions you find in the Undo class and collapses them into one operation
    /// when the block is complete
    /// Proper usage of this class is:
    /// using (var undoBlock = new FixUnityUndoBlock("Desired Undo Message"))
    /// {
    ///     undoBlock.yourCodeToUndo()
    /// }
    ///
    ///
    /// Based on UndoBlock2023_06_25
    /// </summary>
    public class FixUnityUndoBlock : IDisposable
    {
        string m_UndoLabel;
        int m_UndoGroup;
        bool m_DisposedValue; // To detect redundant calls of Dispose
        bool m_TestMode;

        bool m_Dirty;

        /// <summary>
        /// Initialize a new UndoBlock
        ///
        /// BUG in Unity's original: you have to MANUALLY specify that TestRunner is running - because Undo.SetParent is broken, gives incorrect results when TestRunner is running EditMode tests
        /// </summary>
        /// <param name="undoLabel">The label to apply to the undo group created within this undo block</param>
        /// <param name="testMode">Whether this is part of a test run - NOTE this is auto-detected and FORCED to 'true' where possible</param>
        public FixUnityUndoBlock(string undoLabel, bool testMode = false)
        {
#if UNITY_EDITOR
            if( Application.isEditor )
            {
                m_Dirty = false;
                m_TestMode = testMode;

                /** Workaround for Unity's bad programming: detect that Unity's TestRunner is running, and force m_TestMode to true */
                if( Application.isEditor &&
                    !Application.isPlaying && !m_TestMode
                    && Environment.StackTrace.Contains( "UnityEngine.TestRunner" ) )
                    m_TestMode = true;
                
                if( !Application.isPlaying && !m_TestMode )
                {
                    Undo.IncrementCurrentGroup();
                    m_UndoGroup = Undo.GetCurrentGroup();
                    Undo.SetCurrentGroupName(undoLabel);
                    m_UndoLabel = undoLabel;
                }
                else
                    m_UndoGroup = -1;
            }
            else
                m_UndoGroup = -1;
#else
            m_UndoGroup = -1;
#endif
        }

        /// <summary>
        /// Register undo operations for a newly created object.
        /// </summary>
        /// <param name="objectToUndo">The object that was created.</param>
        public void RegisterCreatedObject(UnityObject objectToUndo)
        {
#if UNITY_EDITOR
            if( Application.isEditor )
            {
                if( !Application.isPlaying && !m_TestMode )
                {
                    Undo.RegisterCreatedObjectUndo(objectToUndo, m_UndoLabel);
                    m_Dirty = true;
                }
            }
#endif
        }

        public GameObject CreateGameObjectAsChildOf( string name, GameObject parent )
        {
            return CreateGameObjectAsChildOf( name, parent!=null ? parent.transform : null );
        }
        public GameObject CreateGameObjectAsChildOf( string name, Transform parent )
        {
            return CreateGameObject( name, parent is RectTransform ? new []{typeof(RectTransform)} : null, parent );
        }

        public GameObject CreateGameObject( string name, Type requiredComponent, Transform parent = null )
        {
            return CreateGameObject( name, new[] { requiredComponent }, parent );
        }
        
        /// <summary>
        /// This is the only correct way of creating GameObject instances in Unity - all others from the Unity libraries
        /// are known (by Unity) to be incorrect and/or broken, but Unity refuses to update their docs (they will not fix
        /// them because it would break existing projects).
        ///
        /// Changed from Unity's in-house version, to make it use the correct argument order that mirrors the core Unity API
        /// </summary>
        /// <param name="requiredComponents">2nd (optional) arg to "new GameObject()", almost only ever used with RectTransform</param>
        /// <param name="parent">Optional parent transform for the new GO</param>
        /// <param name="name">Optional override of the default name for the new GO</param>
        /// <returns></returns>
        public GameObject CreateGameObject( string name, Type[] requiredComponents = null, Transform parent = null )
        {
            if( name == null ) name = "New GameObject";
            
            GameObject @return;
            if( requiredComponents != null )
                @return = new GameObject( name, requiredComponents );
            else
                @return = new GameObject(name);
            
            RegisterCreatedObject(@return);
            
            SetTransformParent(@return.transform, parent, false); // NB: false is required, otherwise Unity will mess up your SCALE

            return @return;
        }

        /// <summary>
        /// Records any changes done on the object after the RecordObject function.
        /// </summary>
        /// <param name="objectToUndo">The reference to the object that you will be modifying.</param>
        public void RecordObject(UnityObject objectToUndo)
        {
#if UNITY_EDITOR
            if( Application.isEditor )
            {
                if( !Application.isPlaying && !m_TestMode )
                    Undo.RecordObject(objectToUndo, m_UndoLabel);
            }
#endif
        }

        /// <summary>
        /// Sets the parent of transform to the new parent and records an undo operation.
        /// </summary>
        /// <param name="transform">The Transform component whose parent is to be changed.</param>
        /// <param name="newParent">The parent Transform to be assigned.</param>
        /// <param name="worldPositionStays">As per the Unity Transform.SetParent() method</param>
        public void SetTransformParent(Transform transform, Transform newParent, bool worldPositionStays = true )
        {
#if UNITY_EDITOR
            if( Application.isEditor &&
                !Application.isPlaying && !m_TestMode )
            {
                /**
                 * Undo.SetTransformParent is using the legacy version of transform.SetParent, which ideally we'd avoid, but we have no alternative API call
                 */
                if( worldPositionStays )
                    Undo.SetTransformParent(transform, newParent, m_UndoLabel);
                else
                {
                    /**
                     * ... but at least we can implement logic for the missing worldPositionStays parameter
                     */
                    Vector3 scale = transform.localScale;
                    Vector3 position = transform.position;
                    Quaternion rotation = transform.rotation;

                    /**
                     * RectTransform has a lot of extra data we need to save and restore after reparenting
                     */
                    RectTransform rt = (transform as RectTransform);
                    Vector2 anchorMax, anchorMin, anchoredPosition, pivot, offsetMin, offsetMax, sizeDelta;
                    anchorMax = anchorMin = anchoredPosition = pivot = offsetMin = offsetMax = sizeDelta = Vector2.zero; // Required by the C# compiler (doesn't do static analysis)
                    if( transform is RectTransform )
                    {
                        anchorMax = rt.anchorMax;
                        anchorMin = rt.anchorMin;
                        anchoredPosition = rt.anchoredPosition;
                        pivot = rt.pivot;
                        offsetMin = rt.offsetMin;
                        offsetMax = rt.offsetMax;
                        sizeDelta = rt.sizeDelta;
                    }

                    Undo.RecordObject(transform, "Reparenting "+transform.name);
                    Undo.SetTransformParent(transform, newParent, m_UndoLabel);

                    /**
                     * Restore Transform:
                     */
                    transform.localScale = scale;
                    transform.position = position;
                    transform.rotation = rotation;
                    
                    /**
                     * Restore RectTransform:
                     */
                    if( transform is RectTransform )
                    {
                        rt.anchorMax = anchorMax;
                        rt.anchorMin = anchorMin;
                        rt.anchoredPosition = anchoredPosition;
                        rt.pivot = pivot;
                        rt.offsetMin = offsetMin;
                        rt.offsetMax = offsetMax;
                        rt.sizeDelta = sizeDelta;
                    }
                }

                m_Dirty = true;
            }
            else
            {
                transform.SetParent( newParent, worldPositionStays );
            }
#else
            transform.SetParent(newParent, worldPositionStays);
#endif
        }
        
        
        /// <summary>
        /// Adds a component to the game object and registers an undo operation for this action.
        /// </summary>
        /// <param name="gameObject">The game object you want to add the component to.</param>
        /// <typeparam name="T">The type of component you want to add.</typeparam>
        /// <returns>The new component</returns>
        public T AddComponent<T>(GameObject gameObject) where T : Component
        {
#if UNITY_EDITOR
            if( Application.isEditor )
            {
                if( !Application.isPlaying && !m_TestMode )
                {
                    m_Dirty = true;
                    return Undo.AddComponent<T>(gameObject);
                }
            }
#endif

            return gameObject.AddComponent<T>();
        }

        public void DestroyComponent<T>(T component) where T : Component
        {
#if UNITY_EDITOR
            if( Application.isEditor )
            {
                if( !Application.isPlaying && !m_TestMode )
                {
                    m_Dirty = true;
                    Undo.DestroyObjectImmediate( component );
                }
                else
                    UnityObject.Destroy( component );
            }
            else
                UnityObject.Destroy( component );
#else
            UnityObject.Destroy( component );
#endif
        }
        
        public void DestroyGameObject( GameObject go )
        {
#if UNITY_EDITOR
            if( Application.isEditor )
            {
                if( !Application.isPlaying && !m_TestMode )
                {
                    m_Dirty = true;
                    Undo.DestroyObjectImmediate( go );
                }
                else
                    UnityObject.Destroy( go );
            }
            else
                UnityObject.Destroy( go );
#else
            UnityObject.Destroy( go );
#endif
        }

        /// <summary>
        /// Dispose of this object
        /// </summary>
        /// <param name="disposing">Whether to dispose this object</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_DisposedValue)
            {
                if (disposing && m_UndoGroup > -1)
                {
#if UNITY_EDITOR
                    if( Application.isEditor )
                    {
                        if( !Application.isPlaying && !m_TestMode )
                        {
                            Undo.CollapseUndoOperations(m_UndoGroup);
                            if( m_Dirty )
                            {
                                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                            }
                        }

                        m_Dirty = false;
                    }
#endif
                }

                m_DisposedValue = true;
            }
        }

        /// <summary>
        /// This code added to correctly implement the disposable pattern.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
    }
}