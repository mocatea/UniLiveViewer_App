﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniLiveViewer
{
    public class RandomLight : StageCharaObserver
    {
        [SerializeField] private Transform[] lights;
        [SerializeField] private readonly float MAX_INTERVAL = 0.25f;
        [SerializeField] private float MAXLIFETIME = 1.00f;
        private float interval;
        private float[] timer;

        protected override void Init()
        {
            interval = MAX_INTERVAL;
            timer = new float[lights.Length];
            for (int i = 0; i < timer.Length; i++)
            {
                timer[i] = MAXLIFETIME;
            }
        }

        // Update is called once per frame
        void Update()
        {
            interval -= Time.deltaTime;
            if (interval < 0)
            {
                interval = MAX_INTERVAL;

                int index = Random.Range(0, lights.Length);
                if (!lights[index].gameObject.activeSelf) lights[index].gameObject.SetActive(true);
                lights[index].localRotation = Quaternion.Euler(new Vector3(0,0,Random.Range(-205,-155)));
            }

            for (int i = 0;i< lights.Length; i++)
            {
                if(lights[i].gameObject.activeSelf)
                {
                    timer[i] -= Time.deltaTime;
                    if (timer[i] < 0)
                    {
                        timer[i] = MAXLIFETIME;
                        lights[i].gameObject.SetActive(false);
                    }
                }
            }

        }
    }
}
