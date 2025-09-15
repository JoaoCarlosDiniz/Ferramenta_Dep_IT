# 🖥️ Utilitário de Suporte Windows (AD/NTP/Manutenção)

## 📌 Visão geral
Aplicação **Windows Forms (C# + DevExpress)** para suporte de TI em máquinas Windows integradas em domínio.  
Inclui ferramentas para diagnóstico e correção de problemas comuns: domínio Active Directory, sincronização de hora via NTP, manutenção do Windows, correção de CredSSP no RDP, atualização de aplicativos com **Winget**, reset do spooler de impressão, utilitários de rede e alternância de tema claro/escuro.

---

## ⚙️ Pré-requisitos
- .NET Framework (WinForms)
- **DevExpress WinForms** (Ribbon UI e Skins)
- Execução como **Administrador**
- **Winget** instalado no sistema
- Acesso ao **domínio AD** e **servidores NTP**

---

## 🔑 Configuração
O ficheiro `App.config` deve conter:

```xml
<appSettings>
  <add key="NomeDominio" value="MEU_DOMINIO" />
  <add key="Administrador" value="Administrador" />
  <add key="SenhaAdministrador" value="senhaSegura" />
  <add key="ListServerDC" value="dc01;dc02" />
  <add key="ServidorGateway" value="ntp01;ntp02" />
</appSettings>
```

- **NomeDominio** → Nome do domínio AD  
- **Administrador** → Conta de administrador do domínio (sem domínio na frente)  
- **SenhaAdministrador** → Senha da conta (⚠️ armazenar em claro é risco de segurança)  
- **ListServerDC** → Lista de controladores de domínio (`;` separados)  
- **ServidorGateway** → Lista de servidores NTP (`;` separados)  

> 🔒 **Segurança**: não recomendado guardar senha em texto claro. Considere usar DPAPI, Credential Manager ou GMSA.

---

## 🚀 Funcionalidades

### 🏢 Domínio
- Detecta se o computador já está no **domínio**  
- Se não estiver, permite **ingressar no AD** (via WMI `JoinDomainOrWorkgroup`)  
- Solicita **reinicialização** após join  

### ⏱️ Sincronização de hora
- Seleciona servidor **NTP/gateway** configurado  
- Executa `w32tm` para forçar sincronização  
- Mostra **status** e **fonte** da hora  

### 🗑️ Windows – Testes
- **Limpeza** da pasta `%TEMP%`  
- **CHKDSK** (agendado para próximo boot)  
- **SFC /SCANNOW** (executado imediato)  

### 🔐 RDC – CredSSP
- Aplica correção no Registro:  
  `AllowEncryptionOracle = 2`  

### 👤 Utilizador – Testes
- Atualização de **apps** via:  
  `winget upgrade --all`  

### 🖨️ Impressão
- Reinicia serviço **spooler**  
- Limpa a pasta de **impressões em fila**  

### 🌐 Rede – Testes
- **Ping** ao DC selecionado  
- **Flush DNS** (`ipconfig /flushdns`)  
- **Limpar ARP** (`arp -d *`)  
- **Reset NetBIOS cache** (`nbtstat -R`)  

### 🔄 Reset de conta da máquina
- Executa `netdom resetpwd` para sincronizar senha da conta do computador no domínio  

### 🎨 Tema
- Alterna **tema claro/escuro** (DevExpress skins)  
- Troca **imagem de fundo**:  
  - `Imgs\FundoEscuro.png`  
  - `Imgs\FundoClaro.png`  

---

## 🛠️ Fluxo de uso
1. Configurar o `App.config`  
2. Executar o programa como **Administrador**  
3. Testar ou ingressar no **domínio**  
4. Sincronizar **hora** com servidor NTP  
5. Efetuar **limpeza/manutenção** (TEMP, CHKDSK, SFC)  
6. Aplicar correção do **CredSSP** (se necessário)  
7. Atualizar apps com **Winget**  
8. Resetar **spooler de impressão** em caso de problemas  
9. Testar **rede** e limpar caches  
10. Ajustar o **tema** conforme preferência  

---

## ⚠️ Limitações
- Algumas operações requerem **reinício** (ex.: CHKDSK)  
- **Senha em claro** no App.config é vulnerável  
- Necessário caminho de imagens em `Imgs\`  
- Mensagens estão em **Português (PT-PT)**  

---

## 📜 Licença
[Definir licença aqui]
