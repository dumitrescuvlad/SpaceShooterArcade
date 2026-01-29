## Gameplay

Il giocatore controlla una nave spaziale in un ambiente 2D, affrontando ondate di nemici che attaccano progressivamente.  
Durante la partita è possibile migliorare le proprie abilità tramite un sistema di punti, gestendo risorse come salute, scudi e surriscaldamento dell’arma.

---

## Funzionalità principali

### Movimento e combattimento

- Movimento fluido della nave in 2D
- Sistema di shooting continuo
- Proiettili con velocità e danno modificabili
- Nemici con comportamento e shooting autonomo

---

### Sistema di calore (Overheat)

- Ogni colpo genera calore
- Raggiunto il limite massimo, l’arma si blocca temporaneamente
- Raffreddamento automatico nel tempo
- Bilanciamento pensato per favorire un gameplay strategico

---

### Salute e scudi

- Sistema di punti vita (HP)
- Sistema di scudi separato dagli HP
- Pickup di scudi che ripristinano unità singole
- I pickup non vengono raccolti se gli scudi sono già al massimo

---

### Nemici

- Spawn ritardato all’inizio della partita
- Spawn a distanza di sicurezza dal giocatore
- Nemici che sparano solo quando il player è nel raggio
- Colori casuali dei nemici (rosso, blu, verde, viola, rosa) senza variazioni di statistiche

---

### Sistema di punteggio

- Incremento del punteggio alla distruzione dei nemici
- Visualizzazione del punteggio in tempo reale
- Salvataggio dell’ultimo punteggio a fine partita

---

### Sistema di Abilità (AP)

- Ogni 5 nemici eliminati si ottiene 1 Ability Point
- I punti possono essere spesi durante la partita tramite input da tastiera
- Potenziamenti disponibili:
  - Aumento del danno dei proiettili
  - Aumento della velocità dei proiettili
  - Aumento della velocità della nave
  - Abilità di congelamento dei nemici

---

### Freeze Ability

- Attivabile consumando Ability Points
- Congela tutti i nemici per 5 secondi
- Il giocatore può continuare a muoversi e sparare
- I nemici riprendono normalmente dopo la fine dell’effetto

---

### Reset a fine partita

- Tutti i potenziamenti vengono azzerati alla morte del giocatore
- Ability Points e livelli non persistono tra le partite
- Ogni run è indipendente

---

### UI e flusso di gioco

- Start Screen con pulsante Play
- Visualizzazione dell’ultimo punteggio
- Il gioco resta in pausa fino alla pressione di Play
- Player, HUD e punteggio nascosti nella Start Screen
- Schermata di Game Over con Retry

---

## Struttura del progetto

Assets/
├── Scripts/
│ ├── Camera
│ ├── Combat
│ ├── Core
│ ├── Enemies
│ ├── Pickups
│ ├── Player
│ ├── UI
│ └── Utils
├── Prefab/
├── Scenes/
├── Sprites/
└── Audio/
