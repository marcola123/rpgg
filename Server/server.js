const express = require('express');
const http = require('http');
const socketIo = require('socket.io');
const cors = require('cors');
const path = require('path');
const config = require('./src/config/config');
const database = require('./src/config/database');
const logger = require('./src/utils/logger');

// Importar handlers de eventos
const authEvents = require('./src/sockets/authEvents');
const characterEvents = require('./src/sockets/characterEvents');
const playerEvents = require('./src/sockets/playerEvents');
const combatEvents = require('./src/sockets/combatEvents');
const inventoryEvents = require('./src/sockets/inventoryEvents');

const app = express();

// Middlewares
app.use(cors());
app.use(express.json());
app.use(express.static(path.join(__dirname, 'public')));

const server = http.createServer(app);
const io = socketIo(server, {
  cors: {
    origin: "*",
    methods: ["GET", "POST"]
  }
});

// Conectar ao banco de dados
database.connect();

// Configurar eventos do Socket.IO
io.on('connection', (socket) => {
  logger.info(`Novo cliente conectado: ${socket.id}`);
  
  // Configurar eventos de autenticação
  authEvents(io, socket);
  
  // Configurar eventos de personagem
  characterEvents(io, socket);
  
  // Configurar eventos do jogador
  playerEvents(io, socket);
  
  // Configurar eventos de combate
  combatEvents(io, socket);
  
  // Configurar eventos de inventário
  inventoryEvents(io, socket);
  
  // Evento de desconexão
  socket.on('disconnect', (reason) => {
    logger.info(`Cliente desconectado: ${socket.id} - Motivo: ${reason}`);
    
    // Notificar outros jogadores se este socket tinha um personagem selecionado
    if (socket.characterId && socket.character) {
      socket.to(socket.character.map).emit('player_left', {
        characterId: socket.characterId,
        name: socket.character.name
      });
    }
  });
});

// Rota de health check
app.get('/health', (req, res) => {
  res.status(200).json({ 
    status: 'OK', 
    timestamp: new Date().toISOString(),
    server: config.server.name,
    version: '1.0.0'
  });
});

// Rota para obter informações do servidor
app.get('/server-info', (req, res) => {
  res.status(200).json({
    name: config.server.name,
    version: '1.0.0',
    maxPlayers: config.server.maxPlayers,
    onlinePlayers: io.engine.clientsCount
  });
});

// Manipulador de erros
app.use((err, req, res, next) => {
  logger.error('Erro na aplicação:', err);
  res.status(500).json({ error: 'Erro interno do servidor' });
});

// Rota não encontrada
app.use('*', (req, res) => {
  res.status(404).json({ error: 'Rota não encontrada' });
});

// Iniciar servidor
const PORT = process.env.PORT || 3000;
server.listen(PORT, () => {
  logger.info(`Servidor rodando na porta ${PORT}`);
  logger.info(`Servidor: ${config.server.name}`);
  logger.info(`Máximo de jogadores: ${config.server.maxPlayers}`);
});

// Graceful shutdown
process.on('SIGINT', () => {
  logger.info('Recebido SIGINT. Encerrando servidor...');
  server.close(() => {
    database.connection.end().then(() => {
      logger.info('Servidor encerrado.');
      process.exit(0);
    }).catch(err => {
      logger.error('Erro ao encerrar conexão com banco:', err);
      process.exit(1);
    });
  });
});

module.exports = { app, io };