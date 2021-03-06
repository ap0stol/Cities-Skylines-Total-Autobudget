﻿using System.Collections.Generic;
using ColossalFramework;
using ColossalFramework.UI;

namespace AutoBudget
{
    public abstract class AutobudgetBase
    {
        protected const int oneDayFrames = 585;
        protected int refreshCount = oneDayFrames / 2;
        protected int counter = 0;

        private int prevBudgetDay = 0;
        private int prevBudgetNight = 0;

        public bool Enabled = true;

        public void SetAutobudget()
        {
            if (!Enabled) return;

            if (!Singleton<DistrictManager>.exists || !Singleton<EconomyManager>.exists || !Singleton<SimulationManager>.exists) return;

            EconomyManager em = Singleton<EconomyManager>.instance;
            int budgetDay = em.GetBudget(GetService(), GetSubService(), false);
            int budgetNight = em.GetBudget(GetService(), GetSubService(), true);

            // If not the beginning
            if (prevBudgetDay != 0 && prevBudgetNight != 0)
            {
                // Probably somebody changed budget manually -> disable autobudget
                if (prevBudgetDay != budgetDay || budgetNight != prevBudgetNight)
                {
                    Enabled = false;
                    prevBudgetDay = 0;
                    prevBudgetNight = 0;
                    Mod.UpdateUI();
                    BudgetControlsManager.UpdateControls();
                }
            }

            prevBudgetDay = budgetDay;
            prevBudgetNight = budgetNight;

            if (counter-- <= 0)
            {
                counter = refreshCount;
                setAutobudget();
                prevBudgetDay = 0;
                prevBudgetNight = 0;
            }
        }

        public void SetAutobudgetNow()
        {
            counter = 0;
            SetAutobudget();
        }

        public abstract string GetEconomyPanelContainerName();
        public abstract string GetBudgetItemName();

        public abstract ItemClass.Service GetService();
        public abstract ItemClass.SubService GetSubService();

        protected abstract void setAutobudget();

        protected void setBudget(int newBudget)
        {
            if (newBudget == -1) return;

            SimulationManager sm = Singleton<SimulationManager>.instance;

            // Set the budget sliders
            if (BudgetControlsManager.IsBudgetPanelVisible())
            {
                UISlider slider = BudgetControlsManager.GetBudgetSlider(GetEconomyPanelContainerName(), GetBudgetItemName(), sm.m_isNightTime);
                if (slider != null)
                {
                    if (slider.value != newBudget)
                    {
                        slider.value = newBudget;
                    }
                }
            }

            // Set the budget directly
            Singleton<EconomyManager>.instance.SetBudget(GetService(), GetSubService(), newBudget, sm.m_isNightTime);
        }

        public static IEnumerable<ushort> ServiceBuildingNs(ItemClass.Service service)
        {
            if (Singleton<BuildingManager>.exists)
            {
                BuildingManager bm = Singleton<BuildingManager>.instance;

                FastList<ushort> serviceBuildings = bm.GetServiceBuildings(service);
                if (serviceBuildings != null && serviceBuildings.m_buffer != null)
                {
                    for (int i = 0; i < serviceBuildings.m_size; i++)
                    {
                        ushort n = serviceBuildings.m_buffer[i];
                        if (n == 0) continue;

                        yield return n;
                    }
                }
            }
        }
    }
}
