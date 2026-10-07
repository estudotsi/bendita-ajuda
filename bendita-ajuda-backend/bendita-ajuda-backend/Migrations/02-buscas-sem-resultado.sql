START TRANSACTION;
CREATE TABLE `BuscasNaoEntendidas` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Texto` varchar(100) NOT NULL,
    `Quantidade` int NOT NULL,
    `PrimeiraVez` datetime(6) NOT NULL,
    `UltimaVez` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`)
);

CREATE TABLE `BuscasSemPrestador` (
    `ServicoId` varchar(50) NOT NULL,
    `Cidade` varchar(100) NOT NULL,
    `Uf` varchar(2) NOT NULL,
    `Quantidade` int NOT NULL,
    `UltimaVez` datetime(6) NOT NULL,
    PRIMARY KEY (`ServicoId`, `Cidade`, `Uf`)
);

CREATE UNIQUE INDEX `IX_BuscasNaoEntendidas_Texto` ON `BuscasNaoEntendidas` (`Texto`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261007185849_BuscasSemResultado', '10.0.12');

COMMIT;

