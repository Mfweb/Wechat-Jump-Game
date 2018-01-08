/**
  ******************************************************************************
  * @file    main.c
  * @author  Mfweb
  * @version V1.0
  * @date    2018.1.07
  * @brief
  * @note    微信跳一跳辅助，电容屏触发
  ******************************************************************************
  */

#include "stm32f10x_gpio.h"
#include "stm32f10x.h"
#include "hw_config.h"
#include "stm32fx_delay.h"
#include "stdio.h"

void JumpTime(uint32_t t)
{
	PAout(3) = 0;
	DelayMs(t);
	PAout(3) = 1;
}

int main(void)
{
	int delayTime = 0;
	Init_SysTick();
	USB_Config();//初始化USB
	
	GPIO_QuickInit(GPIOA,GPIO_Pin_4,GPIO_Mode_Out_PP);
	
	//屏幕触发引脚，连接到铝箔纸后贴到屏幕上
	GPIO_QuickInit(GPIOA,GPIO_Pin_3,GPIO_Mode_Out_OD);
	//DelayMs(750);
	PAout(3) = 1;
	while(1)
	{
		//PAout(3) = !PAout(3);
		scanf("%d",&delayTime);
		printf("%d\r\n",delayTime);
		PAout(4) = !PAout(4);
		JumpTime(delayTime);
		//DelayMs(1000);
	}
}
