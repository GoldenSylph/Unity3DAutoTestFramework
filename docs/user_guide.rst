User Guide
===============

.. epigraph::

   *«Intelligence is the ability to avoid doing work, yet getting the work done.»*

   -- Linus Torvalds

The following is a set of guidelines for installing and using the ATF assets for action automation in Unity.

***************
Getting started
***************

1. Installing asset

Short version is just drag-and-drop the .unitypackage file into opened Unity Editor if you have one and it's done.

Or you can hit *Add* in `asset store page <https://assetstore.unity.com/packages/tools/utilities/automated-test-framework-167509>`_ of the asset.
If you have some troubles installing this asset, i suggest you to see this `official guide <https://docs.unity3d.com/Manual/ImportingAssets.html>`_ about importing things in Unity.

2. Preparing scene

Drag-and-drop into your scene *Initializer* prefab in ATF/Prefabs directory of your assets.
Remember, if you destroy this game object the asset functionality won't work.

3. Preparing Unity Editor

All of the management windows are now fully functional.

* To add Integrator Window press: Tools/ATF/Integrator.

* To add Recorder Window press: Tools/ATF/Recorder.

* To add Storage Window press: Tools/ATF/Storage.

Starting from scratch
*********************

Just use *AtfInput* instead of *Input* class and recorder, storage and integrator will work perfectly.

Here is an example of Mover class that will rotate around YZ axis given game object when you hit space key:

.. code-block:: csharp
   :linenos:

   using UnityEngine;

   namespace ATF.Scripts.Example
   {
       public class Mover : MonoBehaviour
       {
           private const float SPEED = 100f;

           private void Update()
           {
               if (AtfInput.GetKey(KeyCode.Space))
               {
                   transform.Rotate(new Vector3(0, 1, 1), Time.deltaTime * SPEED);
               }
           }
       }
   }

Starting from existing project
******************************

Use *AtfInput* class or *AtfInput.Instance* object instead of *Input* class in all further modifications of your code base
and recorder, storage and integrator will work.

*Input* class is sealed so *AtfInput* class and *AtfInput.Instance* object are together to represent it's unified interface.
In other words, if you want to use a certain *Input* method you can access it either through static invokation or through object *AtfInput.Instance* invokation.

For integration in existing project the Integrator Window comes in.
To integrate in the specific files you can populate the list of such files and hit *Integrate* or *Integrate and replace*.
But if you want to integrate automatically without choosing files, just hit *Integrate All*.

.. note:: For further information about integration process just check the Integrator Window part of documentation.

*****************
Integrator Window
*****************

The Integrator Window is used to integrate ATF functionality easily into complete code base.
If you starting from existing project you may want to use one of the options:

Automatic integration
*********************

Just hit *Integrate All* button and it's done.
The algorithm of integration is simple. The integrator selects all of ".cs" files in your project
then in all files it performs regexp search of *Input* class usage turning it in *AtfInput.Instance* class usage.

Integration in specific files
*********************************

There is two modes of integration functionality. First mode allows you to select files in Project window and selected files will be
added to queue on integration. Second mode allows you to add and delete files from queue by entering paths.

To choose modes you can alter state of the checkbox *Manual path choosing*. When it's on first mode is enabled, when it's off second mode is enabled.

.. image:: https://drive.google.com/uc?export=view&id=1YaeLRbwTLj534_K34CZNeJRMdKd5z3hz
    :align: center

You can also delete selected paths from queue.
Just empty your selection in Project window, choose some file you want to delete from queue and hit *Remove path* button.

.. image:: https://drive.google.com/uc?export=view&id=1m_WtDwM5HykNjzsq01QcFbsfAv_SqlWH
    :align: center

To integrate in specific files in second mode you can enter to *.cs file path* path from Assets directory to your script with *Input* class usage.
Clicking *Add path* and *Remove path* buttons you can prepare your own set of scripts for integration by adding or deleting files from queue.

Using *Save paths* and *Load paths* buttons allow you to save and load set of scripts for reintegration if it is needed.

To clear all saved paths you can just remove all paths and hit *Save paths*.
After paths list preparation you have two options:

* *Integrate and replace* button: integrate with replacing contents of chosen files.
* *Integrate* button: Integrate without replacing contents and with creating dublicate file with ATF suffix in file name.

***************
Recorder Window
***************

The Recorder Window is pretty basic to use. The window features three sections: **Recorder state**, **Recording control**, **Replay control**.

.. image:: https://drive.google.com/uc?export=view&id=1tI0mfVrkQaEI_2Yo8ffc1RcCFVznCogW
    :align: center

**Recorder state** is uninteractable section, it just show you the state of the recording.

**Recording control** is interactable section. Here is how to use it:

1. The recorder is a state machine and the name of the record is used as cursor. To set the record name just type to *Name of the recording* field specific name and hit enter. You will see that *Current recording name* has changed.

2. Hit record button *Start* and the window will change like that:

.. image:: https://drive.google.com/uc?export=view&id=1Z6WuHGZYhn43jDh-Sl8daPq9M3spxxz7
    :align: center

The *Stop* and *Pause* buttons are to stop or pause recording. If you click *Pause* the window will change:

.. image:: https://drive.google.com/uc?export=view&id=187qJxOV9HEypAUcdiCSRH91Sr5QgV62d
    :align: center

To continue recording just click *Continue* button.

**Replay control** is also interactable section. Here is how to use it:

1. Now you can play previously recorded actions. Simply type the name of the record a hit enter.
2. Hit replay button *Start* and see how window has changed:

.. image:: https://drive.google.com/uc?export=view&id=1CwTOSKQEuXwWormZc5qXi-Z0qPFLxVth
    :align: center

The *Stop* and *Pause* buttons are to stop or pause replay. If you click *Pause* the window will change:

.. image:: https://drive.google.com/uc?export=view&id=12mgiCaoGOzaEcX7wyQi-Wzg19uAf2hMn
    :align: center

To continue replay just click *Continue* button.

.. note:: If you try to replay a non-existent record it'll do nothing. Also there is no mechanism to reverse replay yet. Please consider this using the asset.

Input Disable Function
**********************

If you want to disable or enable input you can address to Tools/ATF/Utils/Toggle Input or press *Ctrl+I*. Of course it will work only in Play Mode.

This function is to allow interacting with control windows without interacting with application in Play Mode.
For example you want to replay a certain record from certain point of your application but when you move your mouse to click *Start* at the replay control the application is responding and it's state is changing.
To prevent that you can disable input and move your mouse without changing the application state.

Last Input
**********

While recording your new scenario you can see that there is already a record named *Last input* in action storage.
This record contains last state of the requested input and it's content is constantly played where you disable input.
It's also can be used for debug for example.

**************
Storage Window
**************

The Storage Window is to provide persistence management for records you made.

It features two required sections and two optional sections: **Current records**,
**Saved records** and **Current commands and actions queues**, **Saved commands and actions queues**.

Optional means you can hide those sections by checking and unchecking the *Display current details* or *Display saved details* checkboxes.

.. image:: https://drive.google.com/uc?export=view&id=1bD9zECrQPH_fcMgBj4yXcdjrBijP-Aar
    :align: center

**Current records** is section where records that just loaded to RAM are illustrated and they are ready to be recorded again (with erasing previous actions data) and replayed.

**Saved records** is section for records that are saved.

By double-clicking to saved or current record you can see the contents of it in **Current commands and actions queues** section or **Saved commands and actions queues** section. There is the example of such contents:

.. image:: https://drive.google.com/uc?export=view&id=1yAyzEgSsTzZpfgMF7J6RopXjWtusA6iS
    :align: center

Here is how to use the window:

1. The Storage Window is also a state machine. The cursor is *Current recording name* in this window. To set up this cursor you have to just click on any record name in **Current records** or **Saved records** sections.
2. The buttons *Save*, *Load* and *Scrap* are to save to **Saved records** section, load to **Current records** section and scrap record from saved records.
3. To set up cursor in Recorder Window just right-click to any record name in **Current records** section.
4. To export contents of **Saved records** section you can specify an absolute path to ".json" file that will contain the contents and hit *Export saved storage* button.
5. To import contents of **Saved records** section you also specity an absolute path to existing ".json" file of the storage and hit *Import saved storage* button.

.. note:: You cannot set up Recorder Window cursor from Storage Window if record is not loaded to **Current records** section.

**************
Shortcuts
**************

.. image:: https://drive.google.com/uc?export=view&id=15GuuT2ro9NdXty2vRhZfYLYe2Ya_EtZm
    :align: center

These shortcuts are to provide an opportunity to manage recordings without having to move your mouse to the buttons on special windows.
It is useful if your application use mouse position data continuosly in play mode.

1. *Ctrl+I* - Enable/Disable Input;
2. *Ctrl+U* - Start/Stop recording with current record name;
3. *Ctrl+Shift+U* - Pause/Continue current recording;
4. *Ctrl+Y* - Start/Stop playing current record;
5. *Ctrl+Shift+Y* - Pause/Continue current play of record;
