using Roguelike.Core.EventSubscribers;
using Roguelike.Core.Information;
using Roguelike.Core.Movers;
using Roguelike.Core.Openers;
using Roguelike.Core.Pickers;

namespace Roguelike.Core.EventHandlers
{
    public class PlayerEventHandler
    {
        #region Fields
        readonly EntityMover mover;
        readonly IPicker keyPicker;
        readonly IOpener doorOpener;
        private PlayerEventSubscriber subscriber;
        #endregion
        
        #region Methods
        public PlayerEventHandler(EntityMover mover, IPicker keyPicker, IOpener doorOpener)
        {
            this.mover = mover;
            this.keyPicker = keyPicker;
            this.doorOpener = doorOpener;
        }
        public void SetSubscriber(PlayerEventSubscriber value) => subscriber = value;
        
        public void OnMoving(object sender, MovingEventArgs e) => mover.TryToMove(e.Destination);
        public void OnMovingToKey(object sender, MovingEventArgs e) => keyPicker.TryToPickUp(e.Element);
        public void OnMovingToDoor(object sender, MovingEventArgs e) => doorOpener.TryToOpen(e.Element);
        public void OnPlayerDeath(object sender, MovingEventArgs e) =>
            subscriber.UnsubscribeFromInputServiceMovingEvent();
        #endregion
    }
}