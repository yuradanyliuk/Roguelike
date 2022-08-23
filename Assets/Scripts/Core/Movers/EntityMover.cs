﻿using System;
using System.Collections.Generic;
using System.Linq;
using Roguelike.Core.Information;
using UnityEngine;
using Zenject;

namespace Roguelike.Core.Movers
{
    public class EntityMover : MonoBehaviour
    {
        #region Properties
        public int RoomIndex { get; private set; }
        [field: SerializeField] public float MovementStep { get; private set; } = 1f;
        #endregion
        
        #region Fields
        public event EventHandler<MovingEventArgs> MovingToWall;
        public event EventHandler<MovingEventArgs> MovingToKey;
        public event EventHandler<MovingEventArgs> MovingToDoor;
        public event EventHandler<MovingEventArgs> MovingToExit;
        public event EventHandler<MovingEventArgs> ActionCompleted;
        public event Action<int> RoomIndexChanged;
        
        DungeonInfo dungeonInfo;
        #endregion
        
        #region Methods
        [Inject]
        public void Construct(DungeonInfo dungeonInfo) => this.dungeonInfo = dungeonInfo;
        void OnEnable()
        {
            if (dungeonInfo.Rooms != default) 
                RoomIndex = dungeonInfo.GetRoomIndex(transform.position);
        }
        
        void SetRoomIndexIfNew(int roomIndex)
        {
            if (RoomIndex != roomIndex)
            {
                dungeonInfo.ChangeRoomIndex(transform, RoomIndex, roomIndex);
                RoomIndex = roomIndex;
                RoomIndexChanged?.Invoke(roomIndex);
            }
        }
        void Move(Vector3 translation, MovingEventArgs movingArgs)
        {
            transform.Translate(translation * MovementStep);
            SetRoomIndexIfNew(movingArgs.ElementRoomIndex);
        }
        
        public bool TryToMove(Vector3 translation)
        {
            Vector3 destination = transform.position + translation;
            (MovingEventArgs movingArgs, EventHandler<MovingEventArgs> movingEvent) = GetMovingInfo(destination);
            
            if (!movingArgs.IsMovePossible)
                movingEvent?.Invoke(this, movingArgs);
            
            if (movingArgs.IsMovePossible)
                Move(translation, movingArgs);
            else
                movingArgs.Destination = transform.position;
            ActionCompleted?.Invoke(this, movingArgs);
            
            return movingArgs.IsMovePossible;
        }
        public bool IsMovePossible(Vector3 destination) => GetMovingInfo(destination).MovingArgs.IsMovePossible;
        
        (MovingEventArgs MovingArgs, EventHandler<MovingEventArgs> MovingEvent) GetMovingInfo(Vector3 destination)
        {
            int destinationRoomIndex = dungeonInfo.GetRoomIndex(destination);
            (List<Transform>[] ElementsByRoom, EventHandler<MovingEventArgs> MovingEvent)[] roomElementsInfo =
            { 
                (dungeonInfo.KeysByRoom, MovingToKey),
                (dungeonInfo.DoorsByRoom, MovingToDoor),
                (dungeonInfo.ExitsByRoom, MovingToExit),
                (dungeonInfo.WallsByRoom, MovingToWall),
            };
            
            (Transform elementAtDestination, EventHandler<MovingEventArgs> movingEvent) = roomElementsInfo
                .Select(elementsInfo => (ElementAtDestination: elementsInfo.ElementsByRoom[destinationRoomIndex]?
                        .FirstOrDefault(element => element.position == destination), elementsInfo.MovingEvent))
                .FirstOrDefault(elementInfo => elementInfo.ElementAtDestination != default);
            
            MovingEventArgs movingArgs = new MovingEventArgs
            {
                Element = elementAtDestination,
                ElementRoomIndex = destinationRoomIndex,
                IsMovePossible = elementAtDestination == default,
                Destination = destination
            };
            return (movingArgs, movingEvent);
        }
        #endregion
    }
}