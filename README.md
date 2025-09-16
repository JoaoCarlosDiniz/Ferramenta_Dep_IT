### 🎨 Tema Claro
<img width="963" height="626" alt="image" src="https://github.com/user-attachments/assets/5f172f0c-136b-41d8-bb8d-8035481bed23" />

<img width="800" height="398" alt="image" src="https://github.com/user-attachments/assets/ed6dbfa2-11b2-4f20-9784-12b0562e38d2" />

### 🎨 Tema Escuro
<img width="963" height="626" alt="image" src="https://github.com/user-attachments/assets/fae02a02-8c1d-4526-895a-fe93e3129551" />

<img width="800" height="398" alt="image" src="https://github.com/user-attachments/assets/85c9c564-8805-43f9-888b-157f82edabc7" />

# 🖥️ Utilitário de Suporte Windows (AD/NTP/Manutenção)

## 📌 Visão geral
Aplicação **Windows Forms (C# + DevExpress)** para suporte de TI em máquinas Windows integradas em domínio.  
Inclui ferramentas para diagnóstico e correção de problemas comuns: domínio Active Directory, sincronização de hora via NTP, manutenção do Windows, correção de CredSSP no RDP, atualização de aplicativos com Winget, reset do spooler de impressão, utilitários de rede, correções administrativas e alternância de tema claro/escuro. Também apresenta um painel (dashboard) com informações do sistema e indicadores de uso de RAM e disco.

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

## 📊 Painel (Dashboard)

- Na carga da aplicação e no botão Atualizar Painel, a app deteta e apresenta:
    - Nome do PC
    - Processador (via Win32_Processor)
    - RAM total instalada e percentagem de utilização
    - Edição e versão do Windows (Win32_OperatingSystem)
    - Drive C: capacidade total e percentagem de utilização
    - Os indicadores (“ponteiros”) de RAM e Disco existem para tema claro e escuro e são atualizados dinamicamente.

## 🚀 Funcionalidades

### 🏢 Domínio
- Testar domínio: mostra o domínio atual e um DC detetado.
- Ingressar no AD: quando fora do domínio, oferece ingressar via WMI JoinDomainOrWorkgroup com opções:
    - Name = NomeDominio
    - UserName = NomeDominio\Administrador
    - Password = SenhaAdministrador
    - DomainControllerName = <DC escolhido>
    - FJoinOptions = 3 (juntar + criar conta)
- Pede reinicialização após o join.

### ⏱️ Sincronização de hora
- Seleção de servidor a partir de ServidorGateway.
- Sequência automática:
    - net stop w32time
    - w32tm /config /manualpeerlist:"<server>" /syncfromflags:manual /reliable:yes /update
    - net start w32time
    - w32tm /resync /force
    - Consulta w32tm /query /status e /source e mostra o resultado.

### 🗑️ Windows – Testes
- Limpeza de temporários (%TEMP%): apaga ficheiros/pastas e reporta contagem e espaço libertado.
- Verificação de discos:
    - Lista discos, tipo, FS, espaço total/livre.
    - Oferece CHKDSK C: /f (agendado para próximo boot).
    - Oferece SFC /SCANNOW (execução imediata).

### 🔐 RDC – CredSSP
- Aplica correção no Registro:  
    - `AllowEncryptionOracle = 2`  

### 👤 Utilizador – Testes
- Atualização de **apps** via:  
    - `winget upgrade --all`  

### 🖨️ Impressão
- Reinicia o serviço de spooler e limpa a pasta de filas:
    - net stop spooler
    - del %systemroot%\system32\spool\printers\* /Q /F /S
    - net start spooler
- Tenta manter o serviço a correr mesmo em caso de cancelamento/erro. 

### 🌐 Rede – Testes
- Testar comunicação com DC (ping ao DC escolhido; mostra endereço e RTT).
    - Limpar cache DNS: ipconfig /flushdns
    - Limpar ARP: arp -d *
    - Limpar NetBIOS: nbtstat -R

### 🛠️ Correções Administrativas
- Redefinir senha da conta do computador no domínio:
    - netdom resetpwd /server:<DC> /userd:<Domínio\Administrador> /passwordd:<Senha>
- Ativar Acesso Administrativo Remoto (conjunto de ações):
    - Serviços: lanmanserver e lanmanworkstation em auto e iniciar
    - Firewall: ativar grupos “Descoberta de Rede” e “Partilha de Ficheiros e Impressoras”
    - Registo: LocalAccountTokenFilterPolicy = 1 para permitir administração remota com contas locais 

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
- Algumas ações requerem reinício (ex.: CHKDSK).
- As caixas de diálogo são síncronas e bloqueiam até terminar.
- É necessário garantir os ficheiros de imagem em Imgs\.
- Várias operações exigem privilégios de administrador.
- Armazenar credenciais em claro é desaconselhado.

---

## 📜 Licença
Salvo indicação em contrário, os exemplos de código são disponibilizados sob a [Licença MIT](https://pt.wikipedia.org/wiki/Licen%C3%A7a_MIT).
