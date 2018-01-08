#include "stm32f10x_it.h"

void TIM1_UP_IRQHandler(void)
{
	TIM1->SR &= ~TIM_FLAG_Update;//清除中断标志

}

void NMI_Handler(void)
{}
void HardFault_Handler(void)
{while(1){}}
void MemManage_Handler(void)
{while(1){}}
void BusFault_Handler(void)
{while(1){}}
void UsageFault_Handler(void)
{while(1){}}
void SVC_Handler(void)
{}
void DebugMon_Handler(void)
{}
void PendSV_Handler(void)
{}
void SysTick_Handler(void)
{}
