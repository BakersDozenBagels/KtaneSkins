﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

public static class ReflectionHelper
{
    #region Cached Reflection
    public static IList GameplayStateBombs
    {
        get
        {
            if (s_getGameplayStateBombs == null)
            {
                var sceneManagerType = FindTypeInGame("SceneManager");
                var gameplayStateType = FindTypeInGame("GameplayState");
                var instanceGetter = sceneManagerType
                    .GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)
                    .GetGetMethod();
                var gameplayStateGetter = sceneManagerType
                    .GetProperty("GameplayState", BindingFlags.Public | BindingFlags.Instance)
                    .GetGetMethod();
                var bombsGetter = gameplayStateType
                    .GetProperty("Bombs", BindingFlags.Public | BindingFlags.Instance)
                    .GetGetMethod();

                var instance = Expression.Property(Expression.Constant(null, sceneManagerType), instanceGetter);
                var gameplayState = Expression.Property(instance, gameplayStateGetter);
                var bombs = Expression.Property(gameplayState, bombsGetter);

                s_getGameplayStateBombs = Expression.Lambda<Func<IList>>(bombs).Compile();
            }
            return s_getGameplayStateBombs();
        }
    }

    private static Func<IList> s_getGameplayStateBombs;
    #endregion

    #region Helper Methods
    public static readonly BindingFlags Flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

    public static Type FindTypeInGame(string fullName)
    {
        return GameAssembly.GetSafeTypes().FirstOrDefault(t =>
        {
            return t.FullName.Equals(fullName);
        });
    }

    public static Type FindType(string fullName, string assemblyName = null)
    {
        return AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetSafeTypes()).FirstOrDefault(t => t.FullName != null && t.FullName.Equals(fullName) && (assemblyName == null || t.Assembly.GetName().Name.Equals(assemblyName)));
    }

    public static Type FindType(string fullName)
    {
        return AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetSafeTypes()).FirstOrDefault(t =>
        {
            return t.FullName.Equals(fullName);
        });
    }

    public static MethodInfo Method(this Type t, string name)
    {
        return t.GetMethod(name, Flags);
    }

    public static void SetField<T>(this Type t, string name, object o, T value)
    {
        t.GetField(name, Flags).SetValue(o, value);
    }

    public static T Field<T>(this Type t, string name, object o)
    {
        return (T)t.GetField(name, Flags).GetValue(o);
    }

    public static T MethodCall<T>(this Type t, string name, object o, object[] args)
    {
        return (T)t.Method(name).Invoke(o, args);
    }

    public static void MethodCall(this Type t, string name, object o, object[] args)
    {
        t.Method(name).Invoke(o, args);
    }

    private static IEnumerable<Type> GetSafeTypes(this Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            return e.Types.Where(x => x != null);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static Assembly GameAssembly
    {
        get
        {
            if (_gameAssembly == null)
            {
                _gameAssembly = FindType("KTInputManager").Assembly;
            }

            return _gameAssembly;
        }
    }

    private static Assembly _gameAssembly = null;
    #endregion
}