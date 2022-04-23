using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace PublishersFork
{
    /// <summary>
    /// C# engineers almost never need to invoke foreign internal/private code. In the last 5 years Unity's engineers have
    /// developed an obsession with over-using the 'internal' and 'private' keywords.
    /// 
    /// Unity3D was originally built WITHOUT a lot of features and API's specifically because everything was public, so that end-developers could DIY
    /// solutions and fill the gaps. Until/unless the new Unity teams vastly increase the volume and quality of public
    /// APIs they write (and fill in the old gaps) their abuse of internal/private is a problem.
    ///
    /// In the meantime: Unity3D engineers need to invoke private/internals every month or so on any normal mid-sized project.
    /// ...this class makes it vastly easier to work with internal/private.
    ///
    /// NB: a design feature of C# means that when a class extends another class, C#'s own Reflection methods stop working
    /// for private/internal members - there are clear logical reasons for this, but pragmatically it's poor, because
    /// anyone having to invoke them needs to invoke all of them, not just a shallow subset. For normal C# development
    /// this code is written so rarely you wouldn't notice; in Unity3D it's written so often that the 'pragmatic' route
    /// becomes more important than the 'logical' one. The methods below take account of this.
    /// </summary>
    public static class WorkaroundUnityInternal
    {
        /// <summary>
        /// Invoked on an object, it finds the private (or internal) field with the given name - even if that field is
        /// hidden in a superclass/baseclass. It starts with the main class, and iterates up the class hierarchy until
        /// it finds it, or fails.
        /// </summary>
        /// <param name="onObject"></param>
        /// <param name="fieldName"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public static FieldInfo FindPrivateField( this object onObject, string fieldName )
        {
            return FindPrivateField( onObject, fieldName, onObject.GetType() );
        }

        /// <summary>
        /// Use <see cref="FindPrivateField(object,string)"/> instead - this is the more precise internal version that
        /// is able to selectively target different super-classes while iterating up the hierarchy
        /// </summary>
        /// <param name="onObject"></param>
        /// <param name="fieldName"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public static FieldInfo FindPrivateField( this object onObject, string fieldName, Type objectType )
        {
            FieldInfo result = objectType.GetField( fieldName, BindingFlags.Instance | BindingFlags.NonPublic );
            if( result != null )
                return result;

            if( objectType.BaseType != null )
                return onObject.FindPrivateField( fieldName, objectType.BaseType );

            throw new Exception( "Couldn't find field \"" + fieldName + "\" anywhere on type or supertypes of: " + onObject.GetType() );
        }

        /// <summary>
        /// Invoked on an object, it finds the private (or internal) property with the given name - even if that field is
        /// hidden in a superclass/baseclass. It starts with the main class, and iterates up the class hierarchy until
        /// it finds it, or fails.
        /// </summary>
        /// <param name="onObject"></param>
        /// <param name="propertyName"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public static PropertyInfo FindPrivateProperty( this object onObject, string propertyName )
        {
            return FindPrivateProperty( onObject, propertyName, onObject.GetType() );
        }

        /// <summary>
        /// Use <see cref="FindPrivateProperty(object,string)"/> instead - this is the more precise internal version that
        /// is able to selectively target different super-classes while iterating up the hierarchy
        /// </summary>
        /// <param name="onObject"></param>
        /// <param name="propertyName"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public static PropertyInfo FindPrivateProperty( this object onObject, string propertyName, Type objectType )
        {
            PropertyInfo result = objectType.GetProperty( propertyName, BindingFlags.Instance | BindingFlags.NonPublic );
            if( result != null )
                return result;

            if( objectType.BaseType != null )
                return onObject.FindPrivateProperty( propertyName, objectType.BaseType );

            throw new Exception( "Couldn't find property \"" + propertyName + "\" anywhere on type or supertypes of: " + onObject.GetType() );
        }

        /// <summary>
        /// Invoked on an object, it finds the private (or internal) method with the given name - even if that field is
        /// hidden in a superclass/baseclass. It starts with the main class, and iterates up the class hierarchy until
        /// it finds it, or fails.
        /// </summary>
        /// <param name="onObject"></param>
        /// <param name="methodName"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public static MethodInfo FindPrivateMethod( this object onObject, string methodName, Type[] argTypes = null )
        {
            return FindPrivateMethod( onObject, methodName, argTypes, onObject.GetType() );
        }

        /// <summary>
        /// Use <see cref="FindPrivateMethod(object,string)"/> instead - this is the more precise internal version that
        /// is able to selectively target different super-classes while iterating up the hierarchy
        /// </summary>
        /// <param name="onObject"></param>
        /// <param name="methodName"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public static MethodInfo FindPrivateMethod( this object onObject, string methodName, Type[] argTypes, Type objectType )
        {
            MethodInfo result = argTypes == null
                ? objectType.GetMethod( methodName, BindingFlags.Instance | BindingFlags.NonPublic )
                : objectType.GetMethod( methodName, BindingFlags.Instance | BindingFlags.NonPublic, null, argTypes, null );
            if( result != null )
                return result;

            if( objectType.BaseType != null )
                return FindPrivateMethod( onObject, methodName, argTypes, objectType.BaseType );

            throw new Exception( "Couldn't find method \"" + methodName + "\"(" + (argTypes == null ? "" : "[" + argTypes.Length + " params]") + ") anywhere on type or supertypes of: " + onObject.GetType() );
        }

        /// <summary>
        /// You should use <see cref="PrivateFieldValue{T}(object,string)"/> where possible - but frequently Unity's methods
        /// return classes that are themselves incorrectly marked as 'internal', preventing you from compiling them. This
        /// method lets you disable typesafety to workaround Unity's mistakes.
        ///
        /// Returns the value of a private field on the supplied object
        /// </summary>
        /// <param name="onObject"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static object PrivateFieldValue( this object onObject, string fieldName )
        {
            FieldInfo result = onObject.FindPrivateField( fieldName );
            if( result != null )
                return result.GetValue( onObject );

            throw new Exception( "Couldn't find field \"" + fieldName + "\" anywhere on type or supertypes of: " + onObject.GetType() );
        }

        /// <summary>
        /// Returns the value of a private field on the supplied object
        /// </summary>
        /// <param name="onObject"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static T PrivateFieldValue<T>( this object onObject, string fieldName )
        {
            FieldInfo result = onObject.FindPrivateField( fieldName );
            if( result != null )
                return (T) result.GetValue( onObject );

            throw new Exception( "Couldn't find field \"" + fieldName + "\" anywhere on type or supertypes of: " + onObject.GetType() );
        }

        /// <summary>
        /// You should use <see cref="PrivatePropertyValue{T}(object,string)"/> where possible - but frequently Unity's methods
        /// return classes that are themselves incorrectly marked as 'internal', preventing you from compiling them. This
        /// method lets you disable typesafety to workaround Unity's mistakes.
        ///
        /// Returns the value of a private property on the supplied object
        /// </summary>
        /// <param name="onObject"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static object PrivatePropertyValue( this object onObject, string fieldName )
        {
            PropertyInfo result = onObject.FindPrivateProperty( fieldName );
            return result.GetValue( onObject );
        }

        /// <summary>
        /// Returns the value of a private property on the supplied object
        /// </summary>
        /// <param name="onObject"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static T PrivatePropertyValue<T>( this object onObject, string fieldName )
        {
            PropertyInfo result = onObject.FindPrivateProperty( fieldName );
            return (T) result.GetValue( onObject );
        }

        /// <summary>
        /// Invokes a private method on the supplied object
        /// </summary>
        /// <param name="onObject"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public static object PrivateMethod( this object onObject, string methodName )
        {
            MethodInfo result = onObject.FindPrivateMethod( methodName );
            return result.Invoke( onObject, null );
        }

        /// <summary>
        /// Invokes a private method on the supplied object
        /// </summary>
        /// <param name="onObject"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public static object PrivateMethod( this object onObject, string methodName, Type[] argTypes, object[] args )
        {
            MethodInfo result = onObject.FindPrivateMethod( methodName, argTypes );
            return result.Invoke( onObject, args );
        }
    }
}