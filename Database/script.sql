-- =============================================================
-- Prueba técnica AHVA — Base de datos de validación de usuarios
-- Script de referencia. La aplicación crea esta misma estructura
-- automáticamente al arrancar (EnsureCreated + seeder), por lo que
-- NO es necesario ejecutarlo a mano.
-- =============================================================

IF DB_ID('GestionUsuariosDb') IS NULL
    CREATE DATABASE GestionUsuariosDb;
GO

USE GestionUsuariosDb;
GO

IF OBJECT_ID('dbo.Usuarios') IS NULL
BEGIN
    CREATE TABLE dbo.Usuarios (
        Id                 INT IDENTITY (1, 1) PRIMARY KEY,
        TipoDocumento      NVARCHAR(3)   NOT NULL,          -- DNI | CE
        NumeroDocumento    NVARCHAR(12)  NOT NULL,
        PasswordHash       NVARCHAR(100) NOT NULL,          -- BCrypt
        Nombres            NVARCHAR(100) NOT NULL,
        PrimerApellido     NVARCHAR(60)  NOT NULL,
        SegundoApellido    NVARCHAR(60)  NOT NULL,
        FechaNacimiento    DATETIME2     NOT NULL,
        Nacionalidad       NVARCHAR(40)  NOT NULL,
        Sexo               NVARCHAR(20)  NOT NULL,
        CorreoPrincipal    NVARCHAR(120) NOT NULL,
        CorreoSecundario   NVARCHAR(120) NULL,
        TelefonoMovil      NVARCHAR(20)  NOT NULL,
        TelefonoSecundario NVARCHAR(20)  NULL,
        TipoContratacion   NVARCHAR(30)  NOT NULL,
        FechaContratacion  DATETIME2     NOT NULL,
        Cargo              NVARCHAR(80)  NOT NULL,
        Entidad            NVARCHAR(80)  NOT NULL,
        Rol                NVARCHAR(40)  NOT NULL,
        Activo             BIT           NOT NULL DEFAULT 1,
        IntentosFallidos   INT           NOT NULL DEFAULT 0, -- CVF: Contador de Validaciones Fallidas
        BloqueadaHasta     DATETIME2     NULL                -- bloqueo temporal (15 min al 5.º fallo)
    );

    CREATE UNIQUE INDEX IX_Usuarios_TipoDocumento_NumeroDocumento
        ON dbo.Usuarios (TipoDocumento, NumeroDocumento);
END
GO

-- Utilidad para pruebas: desbloquear una cuenta y reiniciar su CVF
-- UPDATE dbo.Usuarios SET IntentosFallidos = 0, BloqueadaHasta = NULL WHERE NumeroDocumento = '07079879';
