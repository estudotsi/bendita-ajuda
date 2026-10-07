CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) NOT NULL,
    `ProductVersion` varchar(32) NOT NULL,
    PRIMARY KEY (`MigrationId`)
);

START TRANSACTION;
CREATE TABLE `CodigosVerificacao` (
    `Id` char(36) NOT NULL,
    `Celular` varchar(20) NOT NULL,
    `CodigoHash` varchar(64) NOT NULL,
    `ExpiraEm` datetime(6) NOT NULL,
    `UsadoEm` datetime(6) NULL,
    `Tentativas` int NOT NULL,
    `CriadoEm` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`)
);

CREATE TABLE `DataProtectionKeys` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `FriendlyName` longtext NULL,
    `Xml` longtext NULL,
    PRIMARY KEY (`Id`)
);

CREATE TABLE `Usuarios` (
    `Id` char(36) NOT NULL,
    `Nome` varchar(100) NOT NULL,
    `Celular` varchar(20) NULL,
    `CelularConfirmado` tinyint(1) NOT NULL,
    `Email` varchar(254) NULL,
    `GoogleId` varchar(100) NULL,
    `Papel` int NOT NULL,
    `CriadoEm` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`)
);

CREATE INDEX `IX_CodigosVerificacao_Celular_CriadoEm` ON `CodigosVerificacao` (`Celular`, `CriadoEm`);

CREATE UNIQUE INDEX `IX_Usuarios_Celular` ON `Usuarios` (`Celular`);

CREATE UNIQUE INDEX `IX_Usuarios_Email` ON `Usuarios` (`Email`);

CREATE UNIQUE INDEX `IX_Usuarios_GoogleId` ON `Usuarios` (`GoogleId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261007005637_Initial', '10.0.12');

CREATE TABLE `Prestadores` (
    `UsuarioId` char(36) NOT NULL,
    `Cep` varchar(8) NOT NULL,
    `Bairro` varchar(100) NULL,
    `Cidade` varchar(100) NOT NULL,
    `Uf` varchar(2) NOT NULL,
    `Bio` varchar(500) NULL,
    `FotoUrl` varchar(500) NULL,
    `Visivel` tinyint(1) NOT NULL,
    `CriadoEm` datetime(6) NOT NULL,
    PRIMARY KEY (`UsuarioId`),
    CONSTRAINT `FK_Prestadores_Usuarios_UsuarioId` FOREIGN KEY (`UsuarioId`) REFERENCES `Usuarios` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `Servicos` (
    `Id` varchar(50) NOT NULL,
    `Nome` varchar(100) NOT NULL,
    `NomePlural` varchar(100) NOT NULL,
    `PalavrasChave` varchar(2000) NOT NULL,
    `Ordem` int NOT NULL,
    PRIMARY KEY (`Id`)
);

CREATE TABLE `ServicosSugeridos` (
    `Id` char(36) NOT NULL,
    `Descricao` varchar(100) NOT NULL,
    `PrestadorId` char(36) NOT NULL,
    `CriadoEm` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_ServicosSugeridos_Prestadores_PrestadorId` FOREIGN KEY (`PrestadorId`) REFERENCES `Prestadores` (`UsuarioId`) ON DELETE CASCADE
);

CREATE TABLE `PrestadoresServicos` (
    `PrestadorId` char(36) NOT NULL,
    `ServicoId` varchar(50) NOT NULL,
    PRIMARY KEY (`PrestadorId`, `ServicoId`),
    CONSTRAINT `FK_PrestadoresServicos_Prestadores_PrestadorId` FOREIGN KEY (`PrestadorId`) REFERENCES `Prestadores` (`UsuarioId`) ON DELETE CASCADE,
    CONSTRAINT `FK_PrestadoresServicos_Servicos_ServicoId` FOREIGN KEY (`ServicoId`) REFERENCES `Servicos` (`Id`) ON DELETE RESTRICT
);

INSERT INTO `Servicos` (`Id`, `Nome`, `NomePlural`, `Ordem`, `PalavrasChave`)
VALUES ('eletricista', 'Eletricista', 'Eletricistas', 1, 'tecnico eletrico, tecnico em eletrica, eletrotecnico, eletrica, eletrico, luz, tomada, chuveiro, disjuntor, fio, fiacao, lampada, interruptor, curto, energia, queimou, choque, quadro de luz');
SELECT ROW_COUNT();

INSERT INTO `Servicos` (`Id`, `Nome`, `NomePlural`, `Ordem`, `PalavrasChave`)
VALUES ('encanador', 'Encanador', 'Encanadores', 2, 'bombeiro hidraulico, hidraulica, agua, pia, vazamento, vazando, vaza, cano, torneira, descarga, privada, vaso sanitario, entupido, entupida, entupiu, esgoto, caixa d agua, registro, ralo');
SELECT ROW_COUNT();

INSERT INTO `Servicos` (`Id`, `Nome`, `NomePlural`, `Ordem`, `PalavrasChave`)
VALUES ('faxina', 'Faxina', 'Profissionais de faxina', 3, 'faxineira, faxineiro, diarista, limpeza, limpar, passar roupa, lavar, sujeira');
SELECT ROW_COUNT();

INSERT INTO `Servicos` (`Id`, `Nome`, `NomePlural`, `Ordem`, `PalavrasChave`)
VALUES ('jardineiro', 'Jardineiro', 'Jardineiros', 6, 'jardinagem, jardim, grama, planta, plantas, poda, podar, arvore, mato, quintal, rocar, horta');
SELECT ROW_COUNT();

INSERT INTO `Servicos` (`Id`, `Nome`, `NomePlural`, `Ordem`, `PalavrasChave`)
VALUES ('montador-de-moveis', 'Montador de móveis', 'Montadores de móveis', 7, 'montar, montagem, movel, moveis, guarda roupa, armario, cama, estante, desmontar, prateleira, rack, mudanca');
SELECT ROW_COUNT();

INSERT INTO `Servicos` (`Id`, `Nome`, `NomePlural`, `Ordem`, `PalavrasChave`)
VALUES ('pedreiro', 'Pedreiro', 'Pedreiros', 4, 'obra, reforma, parede, piso, reboco, muro, telhado, goteira, rachadura, azulejo, cimento, construcao, calcada, laje');
SELECT ROW_COUNT();

INSERT INTO `Servicos` (`Id`, `Nome`, `NomePlural`, `Ordem`, `PalavrasChave`)
VALUES ('pintor', 'Pintor', 'Pintores', 5, 'pintura, pintar, tinta, descascando, mofo, textura, grafiato, verniz');
SELECT ROW_COUNT();

INSERT INTO `Servicos` (`Id`, `Nome`, `NomePlural`, `Ordem`, `PalavrasChave`)
VALUES ('tecnico-ar-condicionado', 'Técnico de ar-condicionado', 'Técnicos de ar-condicionado', 8, 'ar, ar condicionado, split, climatizacao, nao gela, gelando, refrigeracao');
SELECT ROW_COUNT();


CREATE INDEX `IX_Prestadores_Uf_Cidade_Bairro` ON `Prestadores` (`Uf`, `Cidade`, `Bairro`);

CREATE INDEX `IX_PrestadoresServicos_ServicoId` ON `PrestadoresServicos` (`ServicoId`);

CREATE UNIQUE INDEX `IX_Servicos_Nome` ON `Servicos` (`Nome`);

CREATE INDEX `IX_ServicosSugeridos_CriadoEm` ON `ServicosSugeridos` (`CriadoEm`);

CREATE INDEX `IX_ServicosSugeridos_PrestadorId` ON `ServicosSugeridos` (`PrestadorId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261007020951_Second', '10.0.12');

COMMIT;

