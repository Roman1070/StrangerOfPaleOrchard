using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyHealthUIController : GameUiControllerBase
{
    private NPCContainer _container;
    private List<EnemyNPCView> _enemiesContainer;

    public EnemyHealthUIController(SignalBus signalBus, GameCanvas gameCanvas, NPCContainer container) : base(signalBus, gameCanvas)
    {
        _container = container;
        _enemiesContainer = new List<EnemyNPCView>();
        foreach(var npc in _container.NPCs)
        {
            if (npc is EnemyNPCView)
                _enemiesContainer.Add(npc as EnemyNPCView);
        }
    }
}
