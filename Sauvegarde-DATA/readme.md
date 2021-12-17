# VB
Ce projet VB Permet de communiquer avec deux cartes via la liaison série.

Il est nécessaire de réspecter ce protocole de lancement pour avoir un bon fonctionnement:

1 - Brancher la carte Espion via USB
2 - Lancer "U:\ER-main\Sauvegarde-DATA\bin\Debug\SerialCom.exe"
3 - Brancher la carte Maitre via USB
4 - Appuyer sur le bouton "Actualiser"

Ainsi le programme est connecté aux deux cartes.

La commande "P" est evoyé toutes les 5 minutes à la carte Maitre, qui va retourner les tensions et courants des panneaux ainsi que les mesures d'irradiances.