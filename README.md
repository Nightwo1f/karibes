# Karibes - Sistema de Gestão

Sistema de gestão empresarial desenvolvido em WPF (.NET 8) com arquitetura MVVM.

## Estrutura do Projeto

```
Karibes/
├── Karibes.App/          # Projeto principal WPF
│   ├── Views/            # Interfaces XAML
│   ├── ViewModels/       # Lógica de apresentação
│   ├── Models/           # Modelos de dados
│   ├── Services/         # Serviços de negócio
│   ├── Resources/        # Recursos (cores, estilos, ícones)
│   ├── Themes/           # Temas (Light, Dark, Karibes)
│   ├── Data/Excel/       # Armazenamento de dados em Excel
│   └── Utils/            # Utilitários e helpers
└── Karibes.sln          # Solution file
```

## Tecnologias

- **.NET 8.0** - Framework base
- **WPF** - Interface gráfica
- **EPPlus** - Manipulação de arquivos Excel
- **CommunityToolkit.Mvvm** - Padrão MVVM

## Tema Karibes

O tema padrão utiliza a paleta de cores **Preto + Amarelo** (#000000 + #FFD700).

## Funcionalidades

- ✅ Dashboard
- ✅ Gestão de Produtos
- ✅ Gestão de Clientes
- ✅ Gestão de Vendas
- ✅ Gestão Financeira
- ✅ Relatórios
- ✅ Controle de Estoque
- ✅ Backup e Restauração

## Como Executar

1. Abra o projeto no Visual Studio
2. Restaure os pacotes NuGet
3. Compile e execute (F5)

## Licença

Este projeto é privado e de uso interno.





