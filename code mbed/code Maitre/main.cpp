#include "mbed.h"
#include "string.h"
#define nbrCarteConnecte 11     // A définir selon le nombre de carte connectées sur le système

CAN can1(PA_11, PA_12); //Initialisation du bus CAN
Serial pc(USBTX, USBRX,115200); //Initialisation de la liaison série

char ch; // initialisation d'un caractère de type char

//Initialisation de nos chaînes de caractères
char chaine[10];
char commande[3];
char valeur[6];

CANMessage msg;

// Initialisation notre fonction de demander des données de puissances.
DigitalOut S0(D1);
DigitalOut S1(D6);
DigitalOut S2(D7);

AnalogIn E0(A0);
AnalogIn E1(A1);
AnalogIn E2(A2);
AnalogIn E3(A3);

int i=0;

float tension[5][6];
float val_tension[5][6];
float courant[5];
float val_courant[5];

float coeff_tension = 167.78;
float coeff_courant= 9.115103528;

// Find de l'initialization

void acqusition()
{
    int j;
    for(j=0; j<8; j++) { // balayage des 8 voies du multiplexeur
        switch(j) { // affecter S0 S1 S2
            case 0:// Pour la voie 1 du multiplexeur
                S0=0;
                S1=0;
                S2=0;
                tension[1][1]=E0.read();//MPX1
                tension[2][4]=E1.read();//MPX2
                tension[4][2]=E2.read();//MPX3
                courant[1]=E3.read();//MPX4

                break;
            case 1:// Pour la voie 2 du multiplexeur
                S0=1;
                S1=0;
                S2=0;
                tension[1][2]=E0.read();//MPX1
                tension[2][5]=E1.read();//MPX2
                tension[4][3]=E2.read();//MPX3
                courant[2]=E3.read();//MPX4


                break;
            case 2:// Pour la voie 3 du multiplexeur
                S0=0;
                S1=1;
                S2=0;
                tension[1][3]=E0.read();//MPX1
                tension[3][1]=E1.read();//MPX2
                tension[4][4]=E2.read();//MPX3
                courant[3]=E3.read();//MPX4
                break;


            case 3:// Pour la voie 4 du multiplexeur
                S0=1;
                S1=1;
                S2=0;
                tension[1][4]=E0.read();//MPX1
                tension[3][2]=E1.read();//MPX2
                tension[4][5]=E2.read();//MPX3
                courant[4]=E3.read();//MPX4

                break;
            case 4:// Pour la voie 5 du multiplexeur
                S0=0;
                S1=0;
                S2=1;
                tension[1][5]=E0.read();//MPX1
                tension[3][3]=E1.read();//MPX2

                break;
            case 5:// Pour la voie 6 du multiplexeur
                S0=1;
                S1=0;
                S2=1;
                tension[2][1]=E0.read();//MPX1
                tension[3][4]=E1.read();//MPX2

                break;
            case 6:// Pour la voie 7 du multiplexeur
                S0=0;
                S1=1;
                S2=1;
                tension[2][2]=E0.read();//MPX1
                tension[3][5]=E1.read();//MPX2

                break;
            case 7:// Pour la voie 8 du multiplexeur
                S0=1;
                S1=1;
                S2=1;
                tension[2][3]=E0.read();//MPX1
                tension[4][1]=E1.read();//MPX2

        }//fin switch

    }
    for(int i = 1; i<5; i++) {//transforme la valeur de tension recu en Volt
        for(int k=1; k<6; k++) {
            val_tension[i][k] = tension[i][k]*coeff_tension;
        }
    }
    for(int i=1; i<5; i++) {//affiche la tension en Volt
        for(int k=1; k<6; k++) {
            pc.printf("V%d.%d = ", i,k);
            pc.printf("%lf\r\n", val_tension[i][k]);
            
            // envoie des données de tension sur le can 
            msg.id=6;           //envoie du message avec ID 6 
            msg.data[0]=i;      //data 0 num string
            msg.data[1]=k;      //data 1 num panneau
            msg.data[2]= (char)((int)(val_tension[i][k]*100)/256);      // data 2 tension pfort
            msg.data[3]= (char)((int)val_tension[i][k]*100)%256;      //data 3 tension pfaible
            msg.len=4;
            wait_ms(200);
            can1.write(msg);    //ecriture du message sur le bus CAN
            //fin code envoie des données de tension 
            
        }
    }

    for(int i=1; i<5; i++) {//transforme la valeur de courant recu en Ampere
        val_courant[i]= coeff_courant*courant[i];
    }

    for(int i=1; i<5; i++) {// affiche le courant en Ampere
        pc.printf("I%d = ", i);
        pc.printf("%lf\r\n", val_courant[i]);
    }

    // pc.printf("--Fin--");
}

void reception(char ch)     //fonction de reception des caracteres sur la liaison série
{
    Timer t;
    int nbrCartesVerif = 0;
    static int ichaine=0;

    char trame_irradiance[2]; // trame irradiance sur 2 octet
    char flG;

    if ((ch == 13) or (ch == 10)) {     // si le caractère ch = 13 ou 10
        chaine[ichaine]='\0';           //fin de chaine
        pc.printf("\r\n");


        strcpy(commande, strtok(chaine, " "));  //copie chaine du debut jusqu'a un espace => commande
        strcpy(valeur,  strtok(NULL, " ")); //copie chaine depuis un espace jusqu'a la fin => valeur

        pc.printf("Commande %s \r\n",commande);


        if (strcmp(commande,"N")==0) {      //lecture du caractère "N"
            pc.printf("Demande numero de carte \r\n");
            can1.write(CANMessage(0,0, 0)); //ecriture du message sur le bus CAN avec ID 0
            t.start();
            while (t.read() < 1) {
                if(can1.read(msg)) {
                    if(msg.id==10) {
                        nbrCartesVerif++;
                        pc.printf("Reception can ID : %d Nombre de cartes : %d \r\n", msg.data[0], nbrCartesVerif);
                        //pc.printf("Nombre carte verif %d \r\n", nbrCartesVerif);
                    }
                }
            }
            t.stop();
            t.reset();
        }
        if (strcmp(commande,"M")==0) {      //lecture du caractère "M"
            pc.printf("Carte Meteo \r\n");
            can1.write(CANMessage(3,0, 0)); //ecriture du message sur le bus CAN avec ID 3

        }

        if (strcmp(commande,"P")==0) {      //lecture du caractère "P"
            pc.printf("Courant et Tension du champs panneau PV\r\n");
            acqusition();
            
            can1.write(CANMessage(5,0, 0)); // demande mesure irradiation

            wait(0.1);

            flG=0;
            while(flG==0) {
                if (can1.read(msg)) {

                    if (msg.id==51) {
                        
                        pc.printf("Gpoa %d\n\r", (msg.data[0]*256+msg.data[1])/10);

                        flG=1;
                    }
                }
            }
            
            pc.printf("Demande temperature VI \r\n");
            msg.id=1;
            msg.len=0;
            can1.write(msg);    //ecriture du message sur le bus CAN  

        }

        if (strcmp(commande,"I")==0) {      //lecture du caractère "I"
            // pc.printf("Demande Irradiance Gpoa\r\n");
            can1.write(CANMessage(5,0, 0)); //ecriture du message sur le bus CAN avec ID 5

            wait(0.1);

            flG=0;
            while(flG==0) {
                if (can1.read(msg)) {

                    if (msg.id==51) {

                        pc.printf("Gpoa %d\n\r", (msg.data[0]*256+msg.data[1])/10);

                        flG=1;
                    }
                }
            }
        }
        if(strcmp(commande,"C")==0) { //  lecture caractère C
            pc.printf("Demande caractéristiques VI \r\n");
            msg.id=2;
            msg.len=0;
            can1.write(msg);    //ecriture du message sur le bus CAN          
        }
        if(strcmp(commande,"T")==0) { //  lecture caractère T
            pc.printf("Demande temperature VI \r\n");
            msg.id=1;
            msg.len=0;
            can1.write(msg);    //ecriture du message sur le bus CAN          
        }


        if (strcmp(commande,"A")==0) {

            //lecture du caractère "A"
            pc.printf("Carte shutdown marche \r\n");
            msg.id=4;           //envoie du message avec ID 4
            msg.data[0]=1;      //envoie de la valeur 1 sur la data0
            msg.len=1;
            wait_ms(200);
            can1.write(msg);    //ecriture du message sur le bus CAN
        }
        if (strcmp(commande,"E")==0) {      //lecture du caractère "E"
            pc.printf("Carte shutdown arret \r\n");
            msg.id=4;           //envoie du message avec ID 4
            msg.data[0]=0;      //envoie de la valeur 0 sur la data0
            msg.len=1;
            wait_ms(200);
            can1.write(msg);    //ecriture du message sur le bus CAN
        }
        if (strcmp(commande,"V")==0) {      //lecture du caractère "V"

            pc.printf("Carte shutdown marche \r\n");

            msg.id=4;           //envoie du message avec ID 4
            msg.data[0]=1;      //envoie de la valeur 1 sur la data0
            msg.len=1;
            wait_ms(200);
            can1.write(msg);    //ecriture du message sur le bus CAN*/

            wait_ms(500);

            //pc.printf(" Carte  \r\n");
            can1.write(CANMessage(0,0, 0)); //ecriture du message sur le bus CAN avec ID 0
            t.start();
            while (t.read() < 1) {
                if(can1.read(msg)) {
                    if(msg.id==10) {
                        nbrCartesVerif++;
                        pc.printf("Reception can ID : %d Nombre de cartes : %d \r\n", msg.data[0], nbrCartesVerif);
                        //pc.printf("Nombre carte verif %d \r\n", nbrCartesVerif);
                    }
                }
            }
            t.stop();
            t.reset();
            if(nbrCartesVerif!=nbrCarteConnecte) {
                pc.printf("Carte shutdown arret \r\n");
                msg.id=4;           //envoie du message avec ID 4
                msg.data[0]=0;      //envoie de la valeur 0 sur la data0
                msg.len=1;
                wait_ms(200);
                can1.write(msg);    //ecriture du message sur le bus CAN
            }
        }
        ichaine=0;              //remise à zero de la chaine


    } else {
        chaine[ichaine]=ch; //le caractere recu est stocke dans le tableau chaine qui s'incremente
        chaine[ichaine+1]='\0';

        ichaine++;
    }
}




// Fonction main
int main()
{
    int i;
    int numCarte=6; // création numéro carte maître
    pc.printf("main Maitre()\n\r");

    while(1) {                      //boucle infini
        if (pc.readable()!=0) {     //si le pc li quelque chose sur la liaison série
            reception(pc.getc());
        }



        if(can1.read(msg)) {                        //si un message est lu sur le bus CAN

            pc.printf("reception can %d ", msg.id);
            for (i=0; i<msg.len; i++) {
                pc.printf(" %d ", msg.data[i]);     //affiche les datas du message lu
            }
            pc.printf("\r\n");                      //retour au début et saut de ligne*/



            if (msg.id==0) {  // si l'ID de message recu est 0 (demande de numero de carte)
                msg.id=10;                // création d'un message CAN d'ID 10
                msg.data[0] = numCarte;   // renvoie du numeros de carte definie au debut sur la data0
                msg.len=1;                // longueur du message
                wait_ms(numCarte*100);     // temporisation de pour ne pas supperposer plusieurs messages
                can1.write(msg);          // Ecriture du message et envoi sur le bus CAN

            }


        }
    }
}