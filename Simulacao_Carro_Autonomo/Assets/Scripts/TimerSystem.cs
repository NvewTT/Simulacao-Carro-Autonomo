using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TimerSystem : MonoBehaviour
{
    public static event EventHandler onTick;
    
    private const float TICK_TIMER_MAX = .15f;

    private int tick;
    private float tickTimer;
    // Start is called before the first frame update
    void Start()
    {
        tick = 0;
    }

    // Update is called once per frame
    void Update()
    {
        tickTimer += Time.deltaTime;
        if (tickTimer >= TICK_TIMER_MAX)
        {
            tickTimer -= TICK_TIMER_MAX;
            tick++;
            onTick?.Invoke(this, null);
        }
        
    }
}
