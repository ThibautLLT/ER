#include "mbed.h"


Serial pc(USBTX, USBRX,115200); // tx, rx
CAN canMaitre(PA_11, PA_12);



char chaine[10];
char commande[3];
char valeur[6];
int numCarte=60; // Definition du Numero de la carte a 60

int i=0;

void reception(char ch)
{
    if ((ch == 13) or (ch == 10)) {
        chaine[i] = '\0';

        strcpy(commande, strtok(chaine, " "));
        strcpy(valeur, strtok(NULL, " "));

        if (strcmp(commande, "N") == 0) {
            pc.printf("MessageBusCAN\r\n");
            canMaitre.write(CANMessage(0,0,0));
            
        
            
        } else {
            pc.printf("Commande inconnue\r\n");
        }
        i = 0;
    } else {
        chaine[i] = ch;
        i++;

    }
}


int main()
{
    int i;
    CANMessage msg;

    //pc.printf("Bonjour\r\n"); // afficher Bonjour
    while(1) {
        if (pc.readable()!=0) {
            reception(pc.getc());
        }

        if(canMaitre.read(msg)) {
            pc.printf("MsgId %d",msg.id);
            
            for (i=0; i<msg.len; i++) {
                pc.printf(" Data%d %d",i,msg.data[i]);
            }
            pc.printf("\r\n");
            
         
            if (msg.id==0) {  // si l'ID de message recu est 0 (demande de numero de carte)
                pc.printf("Demande NUM \r\n");
                                
                wait_ms(numCarte*100);     // temporisation de pour ne pas supperposer plusieurs messages
                pc.printf("Fin attente \r\n");
           
             
                msg.id=10;                // création d'un message CAN d'ID 10
                msg.data[0] = numCarte;   // renvoie du numeros de carte definie au debut sur la data0
                msg.len=1;                // longueur du message
                canMaitre.write(msg);          // Ecriture du message et envoi sur le bus CAN
           
            }
            
        }
    }
}
