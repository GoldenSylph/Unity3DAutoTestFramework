Developer Guide
===============

The following is a set of guidelines for creating new systems as extension to current ones and some information about ground architecture of the ATF assets.

********************
Creating new system
********************

Here is some base steps for creating and integrating some new system.

Initializer
********************

The `AtfInitializer class <https://github.com/GoldenSylph/Unity3DAutoTestFramework/blob/master/Assets/ATF/Scripts/AtfInitializer.cs>`_ is for instantiating automatically all *MonoSingleton<T>* instances as **Initializer** game object child
that are marked by AtfSystem attribute.

In context of ATF it is used to instantiate all main systems described below.

.. code-block:: csharp
  :linenos:

  using ATF.Scripts.DI;
  using ATF.Scripts.Helper;

  namespace MyLovely.Namespace
  {
      [AtfSystem]
      public class MyNewSystem : MonoSingleton<MyNewSystem>
      {
          ...
      }
  }

If you now enter the Play Mode the *AtfInitializer* class will instantiate as child of itself your newly created class *MyNewSystem*.

DI Container
********************

DI Container stands for `Dependency Injection Container <https://github.com/GoldenSylph/Unity3DAutoTestFramework/blob/master/Assets/ATF/Scripts/DI/DependencyInjector.cs>`_.
It's a common method to implement the Dependency Inversion Principle (DIP) of SOLID principles.
Usage:

.. code-block:: csharp
   :linenos:

   using ATF.Scripts.DI;
   using ATF.Scripts.Helper;

   namespace MyLovely.Namespace
   {
       public interface IMySystem {
          ...
       }

       [AtfSystem]
       [Injectable]
       public class MyNewSystemA : MonoSingleton<MyNewSystemA>, IMySystem
       {
           ...
       }

       [AtfSystem]
       [Injectable]
       public class MyNewSystemB : MonoSingleton<MyNewSystemB>
       {
           ...
       }

       [AtfSystem]
       [Injectable]
       public class MyNewSystemC : MonoSingleton<MyNewSystemC>, IMySystem
       {
         [Inject(typeof(MyNewSystemA))]
         public static readonly IMySystem SYSTEM_A;

         [Inject(typeof(MyNewSystemB))]
         public static readonly MyNewSystemB SYSTEM_B;

           ...
       }
   }

The main attributes here are *Injectable* and *Inject*.
First is used to appear in queue to dependency injection.
Second is used with one required parameter of *Type* class.
The purpose of the second is to mark field of class that is to be populated by instance of class that has type *Type* via DI Container.

.. note:: Every ATF system has *Injectable* and *AtfSystem* attributes so you can use them in your own systems.

********************
Platform Diagram
********************

.. figure:: https://drive.google.com/file/d/1KIKXtF2D5edlqOYRc4ydmlLeJn3BMYDd/view?usp=sharing
    :align: center
    :alt: Platform diagram

Every block at the ATF platform diagram above is an isolated class group:

* *Custom Unity Input Module* -- an abstraction that combines input management;
* *Custom Input API* -- module that calls native methods on an input request;
* *Custom BaseInput* -- an entity that is an implementation of a data flow processing object across a bridge combining static methods for intercepting/simulating input and wrapped events;
* *Storage* -- a group of classes that is responsible for storing and manipulating recorded actions;
* *Recorder* -- a group of classes that is responsible for recording actions;
* *Custom Editor UI* -- a system of custom windows for managing all processes;
* *PlayerPrefs Save/Load Module* -- system for implementing the module for saving / loading recorded actions based on the standard PlayerPrefs class;
* *Dictionary based Module* -- implementation of the recorded action store abstraction based on the Dictionary data structure;
* *Queue based Recorder Module* -- implementation of the module responsible for recording actions based on the Queue data structure with RLE-compression;

********************
Main systems
********************

The next several titles are describing the base system interfaces of the ATF.

Some of them implement the following interface describing cursor get and set methods.

To check the current realisations just visit our github `page <https://github.com/GoldenSylph/Unity3DAutoTestFramework>`_.

.. code-block:: csharp
   :linenos:

   namespace ATF.Scripts.Helper {
       public interface IAtfGetSetRecordName
       {
           string GetCurrentRecordName();
           void SetCurrentRecordName(string recordName);
       }
   }

Recorder System
********************

The `Recorder System <https://github.com/GoldenSylph/Unity3DAutoTestFramework/blob/master/Assets/ATF/Scripts/Recorder/AtfQueueBasedRecorder.cs>`_ is serving as arbitrator.
It's current realisation based on classic state machine. And it implements the following interface.

.. code-block:: csharp
   :linenos:

   using ATF.Scripts.Helper;

   namespace ATF.Scripts.Recorder
   {
       public interface IAtfRecorder : IAtfGetSetRecordName
       {
           bool IsRecording();
           bool IsPlaying();

           bool IsRecordingPaused();
           bool IsPlayPaused();

           void PlayRecord();
           void PausePlay();
           void ContinuePlay();
           void StopPlay();

           void StartRecord();
           void PauseRecord();
           void ContinueRecord();
           void StopRecord();

           void SetRecording(bool value);
           void SetPlaying(bool value);
           void SetRecordingPaused(bool value);
           void SetPlayPaused(bool value);

           void Record(FakeInput kind, object input, object fakeInputParameter);
       }
   }

Action Storage System
*********************

The `Action Storage System <https://github.com/GoldenSylph/Unity3DAutoTestFramework/blob/master/Assets/ATF/Scripts/Storage/AtfDictionaryBasedActionStorage.cs>`_ is a core of the ATF.
It implements the following interface and stores recorded actions in format of generic type **Dictionary<string, Dictionary<FakeInput, Dictionary<object, AtfActionRleQueue>>>**.

Where string - is name of the record, FakeInput is enum described below, object - is parameter of the input and *AtfActionRleQueue* is basic queue data structure but with RLE-compression of the elements.

.. code-block:: csharp
   :linenos:

   using System.Collections.Generic;
   using ATF.Scripts.Helper;
   using ATF.Scripts.Storage.Utils;
   using UnityEditor.IMGUI.Controls;

   namespace ATF.Scripts.Storage.Interfaces
   {
       public interface IAtfActionStorage : IAtfGetSetRecordName
       {
           object GetPartOfRecord(FakeInput kind, object fakeInputParameter);
           void Enqueue(string recordName, FakeInput kind, object fakeInputParameter, AtfAction atfAction);
           AtfAction Dequeue(string recordName, FakeInput kind, object fakeInputParameter);
           AtfAction Peek(string recordName, FakeInput kind, object fakeInputParameter);
           bool PrepareToPlayRecord(string recordName);
           void ClearPlayStorage();
           void SaveStorage();
           void LoadStorage();
           void ScrapSavedStorage();
           List<TreeViewItem> GetSavedRecordNames();
           List<TreeViewItem> GetCurrentRecordNames();
           List<TreeViewItem> GetCurrentActions(string recordName);
           List<TreeViewItem> GetSavedActions(string recordName);
       }
   }

FakeInput enum is the following:

.. code-block:: csharp
   :linenos:

   public enum FakeInput {
       NONE,
       ANY_KEY_DOWN,
       ANY_KEY,
       GET_AXIS,
       GET_AXIS_RAW,
       GET_BUTTON,
       GET_BUTTON_DOWN,
       GET_BUTTON_UP,
       GET_KEY,
       GET_KEY_DOWN,
       GET_KEY_UP,
       GET_MOUSE_BUTTON,
       GET_MOUSE_BUTTON_DOWN,
       GET_MOUSE_BUTTON_UP
   }

And it's represent any kind of input that we can acquire from *Input* class.

Packer System
********************

The following interface define how to pack and unpack storage data into serializable *Slot* class.

The current realisation of it is using greedy algorithm.

.. code-block:: csharp
   :linenos:

   using System.Collections.Generic;
   using ATF.Scripts.Storage.Utils;
   using UnityEngine;

   namespace ATF.Scripts.Storage.Interfaces
   {
       public interface IAtfPacker
       {
           List<Record> Pack(Dictionary<string, Dictionary<FakeInput,
              Dictionary<object, AtfActionRleQueue>>> input);
           Dictionary<string, Dictionary<FakeInput, Dictionary<object,
              AtfActionRleQueue>>> Unpack(Slot slot);
           string ValidatePacked(List<Record> packed);
       }
   }


Action Storage Saver System
***************************

Because of the potential need in saving storage data in different places (ex. file system, *PlayerPrefs* class, etc.) this interface was created.
T
he current realisation uses *PlayerPrefs* class. You can always expand this on file system for example by implementing this interface with specific file system API usage.

.. code-block:: csharp
   :linenos:

   using System.Collections;
   using System.Collections.Generic;
   using ATF.Scripts.Helper;
   using UnityEditor.IMGUI.Controls;

   namespace ATF.Scripts.Storage.Interfaces
   {
       public interface IAtfActionStorageSaver : IAtfGetSetRecordName
       {
           void SaveRecord();
           void LoadRecord();
           void ScrapRecord();

           IEnumerable GetActions();
           void SetActions(IEnumerable actionEnumerable);
           List<TreeViewItem> GetSavedNames();
           List<TreeViewItem> GetSavedRecordDetails(string recordName);
       }
   }

Integrator System
********************

This interface define methods for automatic integrator system that allow preparing and saving group of selected source files and integrating them.

.. code-block:: csharp
   :linenos:

   using System.Collections.Generic;
   using ATF.Scripts.Helper;

   namespace ATF.Scripts.Integration.Interfaces
   {
       public interface IAtfIntegrator : IAtfGetSetRecordName
       {
           void SetUris(IEnumerable<string> filePaths);
           void Integrate();
           void IntegrateAndReplace();
           void IntegrateAll();
           void SaveUris();
           IEnumerable<string> LoadUris();
       }
   }

.. note:: This current realisations might be changed during the development of the ATF, so in this page only interfaces are illustrated. SOLID rules.
