#include "mbed.h"
#include <time.h>
Serial pc(USBTX, USBRX,115200); // tx, rx

int main()
{
    int i = 0;
    while(true) {
        pc.printf("MsgId 51 Data0 30 Data%d 50\r\n", i++);
        pc.printf("MsgId 52 Data0 30 Data%d 50\r\n", i++);
        pc.printf("MsgId 53 Data0 30 Data%d 50\r\n", i++);
        wait(5);
    }
}

void wait ( int seconds )
{
    clock_t endwait;
    endwait = clock () + seconds * CLOCKS_PER_SEC ;
    while (clock() < endwait) {}
}
