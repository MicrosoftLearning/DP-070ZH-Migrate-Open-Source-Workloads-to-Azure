---
lab:
    title: '将本地 MySQL 数据库迁移到 Azure'
    module: '模块 2：将本地 MySQL 迁移至 Azure Database for MySQL'
---

# 实验室：将本地 MySQL 数据库迁移到 Azure

## 概述

在本实验室中，你将使用在本模块中学习到的信息将 MySQL 数据库迁移到 Azure。为了完全涵盖所学内容，学生将执行两次迁移。第一个是脱机迁移，将本地 MySQL 数据库传输到在 Azure 上运行的虚拟机。第二个是将虚拟机上运行的数据库联机迁移到 Azure Database for MySQL。

你还将重新配置并运行一个使用数据库的示例应用程序，以验证每次迁移后数据库是否正常运行。

## 学习目标

完成本实验室后，你将能够：

1. 执行本地 MySQL 数据库到 Azure 虚拟机的脱机迁移。

1. 将在虚拟机上运行的 MySQL 数据库联机迁移到 Azure Database for MySQL。

## 场景

你是 AdventureWorks 组织的数据库开发人员。十多年来，AdventureWorks 一直将自行车和自行车零件直接销售给最终用户和分销商。他们的系统将信息存储在当前使用 MySQL 运行的数据库中，该数据库位于其本地数据中心中。作为硬件合理化练习的一部分，AdventureWorks 希望将数据库移至 Azure。已要求你执行此迁移。

最初，你决定将数据快速重定位到在 Azure 虚拟机上运行的 MySQL 数据库。这被视为一种低风险的方法，因为它几乎不需要对数据库进行任何更改。但是，此方法确实需要你继续执行与数据库相关的大多数日常监视和管理任务。你还需要考虑 AdventureWorks 的客户群如何变化。最初，AdventureWorks 的目标客户是本地区的客户，但如今，他们已扩展到全球范围。客户可以位于任何地方，而确保查询数据库的客户延迟时间最短是首要问题。你可以对位于其他区域的虚拟机实施 MySQL 复制，但这同样是管理上的开销。

相反，一旦系统在虚拟机上运行，你就可以考虑采用更长远的解决方案；你将数据库迁移到 Azure Database for MySQL。此 PaaS 策略省去了与维护系统相关的大量工作。你可以轻松扩展系统，并添加只读副本以支持全球任何地方的客户。此外，Microsoft 提供了有保证的 SLA 可用性。

## 设置

你有一个内部环境，其中一个现有的 MySQL 数据库包含要迁移到 Azure 的数据。在开始实验室之前，你需要创建一个运行 MySQL 的 Azure 虚拟机，该虚拟机将充当初始迁移的目标。我们提供了一个脚本，用于创建该虚拟机并进行配置。使用以下步骤下载并运行此脚本：

1. 登录到在教室环境中运行的 **LON-DEV-01** 虚拟机。用户名是“**azureuser**”，密码为“**Pa55w.rd**”。

    该虚拟机模拟你的本地环境。它正在运行 MySQL 服务器，该服务器承载着你需要迁移的 AdventureWorks 数据库。

2. 使用浏览器登录至 Azure 门户。

3. 打开“Azure Cloud Shell”窗口。确保你正在运行 **Bash** shell。

4. 如果以前未执行此操作，请克隆包含脚本和示例数据库的存储库。

    ```bash
    git clone https://github.com/MicrosoftLearning/DP-070-Migrate-Open-Source-Workloads-to-Azure workshop 
    ```

5. 移动到“*migration_samples/setup*”文件夹。

    ```bash
    cd ~/workshop/migration_samples/setup
    ```

6. 按如下方式运行“*create_mysql_vm.sh*”开脚本。指定资源组的名称和用于以参数方式承载虚拟机的位置。如果资源组尚不存在，则会创建该资源组。指定你附近的位置，例如“*eastus*”或“*uksouth*”：

    ```bash
    bash create_mysql_vm.sh [resource group name] [location]
    ```

    该脚本大约需要 10 分钟的运行时间。它将在运行时生成大量输出，最后输出新虚拟机的 IP 地址和“**设定完成**”消息。 
7. 记下该 IP 地址。

> [!注意]
> 你需要使用此 IP 地址进行此练习。

## 练习 1：将本地数据库迁移到 Azure 虚拟机

在该练习中，你将执行以下任务：

1. 查看本地数据库。
2. 运行一个要查询数据库的示例应用程序。
3. 执行数据库到 Azure 虚拟机的脱机迁移。
4. 验证 Azure 虚拟机上的数据库。
5. 根据 Azure 虚拟机中的数据库重新配置并测试示例应用程序。

### 任务 1：查看本地数据库

1. 在教室环境中运行的 **LON-DEV-01** 虚拟机上，在在屏幕左侧的“**收藏夹**”工具栏中，单击“**MySQLWorkbench**”。

1. 在“**MySQLWorkbench**”窗口，单击“**LON-DEV-01**”，并单击“**确定**”。

1. 展开“**adventureworks**”，然后展开“**表**”。

1. 右击“**contact**”表，单击“**选择行 - 限制 1000**”，然后在“**contact**”查询窗口，单击“**限制为 1000 行**”，然后单击“**不限制**”。

1. 单击“**执行**”运行查询。它应该返回 19972 行。

1. 在表列表中，右键单击“**Employee**”表，然后单击“**选择行**”。

1. 单击“**执行**”运行查询。它应该返回 290 行。

1. 花几分钟时间浏览数据库内各种表中其他表的数据。

### 任务 2：运行查询数据库的示例应用程序

1. 在 **LON-DEV-01** 虚拟机的在收藏夹栏上，单击“**终端**”打开终端窗口。

1. 在终端窗口中，下载实验室的示例代码。如果出现提示，请在密码中输入“**Pa55w.rd**”：

   ```bash
   sudo rm -rf ~/workshop
   git clone  https://github.com/MicrosoftLearning/DP-070-Migrate-Open-Source-Workloads-to-Azure ~/workshop
   ```

1. 移动到“~/workshop/migration_samples/code/mysql/AdventureWorksQueries”文件夹：

   ```bash
   cd ~/workshop/migration_samples/code/mysql/AdventureWorksQueries
   ```

    此文件夹包含一个示例应用程序，该应用程序运行查询以计算 *adventureworks* 数据库内几个表中的行数。

1. 运行应用程序：

    ```bash
    dotnet run
    ```

    该应用程序应生成以下输出：

    ```text
    Querying AdventureWorks database
    SELECT COUNT(*) FROM product
    504

    SELECT COUNT(*) FROM vendor
    104

    SELECT COUNT(*) FROM specialoffer
    16

    SELECT COUNT(*) FROM salesorderheader
    31465

    SELECT COUNT(*) FROM salesorderdetail
    121317

    SELECT COUNT(*) FROM customer
    19185
    ```

### 任务 3：执行数据库到 Azure 虚拟机的脱机迁移

现在，你已经了解了 adventureworks 数据库中的数据，可以将其迁移到在 Azure 内的虚拟机上运行的 MySQL 服务器。你将使用备份和还原命令将此操作作为脱机任务执行。

> [!注意]
> 如果要联机迁移数据，则可以配置从本地数据库复制到在 Azure 虚拟机上运行的数据库。

1. 在终端窗口中，运行以下命令以备份 *adventureworks* 数据库。请注意，LON-DEV-01 虚拟机上的 MySQL 服务器正在使用端口 3306 进行侦听：

    ```bash
    mysqldump -u azureuser -pPa55w.rd adventureworks > aw_mysql_backup.sql
    ```

1. 使用 Azure Cloud Shell，连接到包含 MySQL 服务器和数据库的虚拟机。将 \<*nn.nn.nn.nn*\> 替换成虚拟机的 IP 地址。如果询问你是否要继续，请键入“**是**”，然后按 Enter。

    ```bash
    ssh azureuser@nn.nn.nn.nn
    ```

1. 键入“**Pa55w.rdDemo**”，然后按 Enter。
1. 连接到 MySQL 服务器：

    ```bash
    mysql -u azureuser -pPa55w.rd
    ```

1. 在 Azure 虚拟机上创建目标数据库：

    ```azurecli
    create database adventureworks;
    ```

1. 退出 MySQL：

    ```bash
    quit
    ```

1. 退出 SSH 会话：

    ```bash
    exit
    ```

1. 使用 mysql 命令将备份还原到新数据库中：

    ```bash
    mysql -h [nn.nn.nn.nn] -u azureuser -pPa55w.rd adventureworks < aw_mysql_backup.sql
    ```

    执行此命令可能需要几分钟。

### 任务 4：验证 Azure 虚拟机上的数据库

1. 运行以下命令以连接到 Azure 虚拟机上的数据库。对于在虚拟机上运行的 MySQL 服务器中的“*azureuser*”用户，其密码是“**Pa55w.rd**”：

    ```bash
    mysql -h [nn.nn.nn.nn] -u azureuser -pPa55w.rd adventureworks
    ```

1. 运行以下查询：

    ```SQL
    SELECT COUNT(*) FROM specialoffer;
    ```

    验证此查询返回 16 行。这与本地数据库中的行数相同。

1. 查询 *vendor* 表中的行数。

    ```SQL
    SELECT COUNT(*) FROM vendor;
    ```

    该表应包含 104 行。

1. 使用 **quit** 命令关闭“*mysql*”实用程序。
1. 切换到“**MySQL Workbench**”工具。
1. 在“**数据库**”菜单，单击“**管理连接**”，然后单击“**新建**”。
1. 单击“**连接**”选项卡。
1. 在“**连接名称**”中键入“**MySQL on Azure**”
1. 输入以下详细信息，然后单击“**测试连接**”：

    | 属性  | 值  |
    |---|---|
    | 主机名 | [nn.nn.nn.nn] |
    | 端口 | 3306 |
    | 用户名 | azureuser |
    | 默认架构 | AdventureWorks |

1. 在密码提示符下，键入**Pa55w.rd**并单击**确定**。
1. 单击“**确定**”，然后单击“**关闭**”。
1. 在**数据库**菜单上，单击**连接到数据库**，选择**Azure 上的 MySQL**，然后单击**确定**。
1. 在“**adventureworks**”中，浏览数据库中的表。这些表应与本地数据库中的表相同。

### 任务 5：根据 Azure 虚拟机上的数据库重新配置和测试示例应用程序

1. 返回到“**终端**”窗口。

1. 使用“*nano*”编辑器打开测试应用程序的 App.config 文件：

    ```bash
    nano App.config
    ```

1. 更改“**ConnectionString**”设置的值，将“**127.0.0.1**”替换为 Azure 虚拟机的 IP 地址。文件内容应如下：

    ```XML
    <?xml version="1.0" encoding="utf-8" ?>
    <configuration>
    <appSettings>
        <add key="ConnectionString" value="Server=nn.nn.nn.nn;Database=adventureworks;Uid=azureuser;Pwd=Pa55w.rd;" />
    </appSettings>
    </configuration>
    ```

    应用程序现在应连接到在 Azure 虚拟机上运行的数据库。

1. 要保存文件并关闭编辑器，请按 ESC，然后按 CTRL+X。出现提示时，请按 Enter 保存更改。

1. 生成并运行该应用程序：

    ```bash
    dotnet run
    ```

    验证应用程序是否成功运行，并为每个表返回与以前相同的行数。

    现在，你已将本地数据库迁移到 Azure 虚拟机，并重新配置了应用程序以使用新数据库。

## 练习 2：执行到 Azure Database for MySQL 的联机迁移

在该练习中，你将执行以下任务：

1. 配置在 Azure 虚拟机上运行的 MySQL 服务器并导出架构。
2. 创建 Azure Database for MySQL 服务器和数据库。
3. 将架构导入目标数据库。
4. 使用数据库迁移服务执行联机迁移
5. 修改数据，并直接转换到新数据库
6. 验证 Azure Database for MySQL 中的数据库
7. 根据 Azure Database for MySQL 中的数据库重新配置和测试示例应用程序。

### 任务 1：配置在 Azure 虚拟机上运行的 MySQL 服务器并导出架构

1. 使用 Web 浏览器，返回到 Azure 门户。

1. 打开“Azure Cloud Shell”窗口。确保你正在运行 **Bash** shell。

1. 连接到运行 MySQL 服务器的 Azure 虚拟机。在以下命令中，将 *nn.nn.nn.nn* 替换成虚拟机的 IP 地址。出现提示时，输入密码“**Pa55w.rdDemo**”：

    ```bash
    ssh azureuser@nn.nn.nn.nn
    ```

1. 验证 MySQL 已正确启动：

    ```bash
    service mysql status
    ```

    如果该服务正在运行，则应该看到类似于以下内容的消息：

    ```text
         mysql.service - MySQL Community Server
           Loaded: loaded (/lib/systemd/system/mysql.service; enabled; vendor preset: enabled)
           Active: active (running) since Mon 2019-09-02 14:45:42 UTC; 21h ago
          Process: 15329 ExecStart=/usr/sbin/mysqld --daemonize --pid-file=/run/mysqld/mysqld.pid (code=exited, status=0/SUCCESS)
          Process: 15306 ExecStartPre=/usr/share/mysql/mysql-systemd-start pre (code=exited, status=0/SUCCESS)
         Main PID: 15331 (mysqld)
            Tasks: 30 (limit: 4070)
           CGroup: /system.slice/mysql.service
                   └─15331 /usr/sbin/mysqld --daemonize --pid-file=/run/mysqld/mysqld.pid
        
            Sep 02 14:45:41 mysqlvm systemd[1]: Starting MySQL Community Server...
            Sep 02 14:45:42 mysqlvm systemd[1]: Started MySQL Community Server.
    ```

1. 使用 mysqldump 实用程序导出源数据库的架构：

    ```bash
    mysqldump -u azureuser -pPa55w.rd adventureworks --no-data > adventureworks_mysql_schema.sql
    ```

1. 在 bash 提示符下，运行以下命令以将“**adventureworks**”的架构导出到名为“**adventureworks_mysql.sql**”的文件

    ```bash
    mysqldump -u azureuser -pPa55w.rd adventureworks > adventureworks_mysql.sql
    ```

1. 断开与虚拟机的连接并返回到 Cloud Shell 提示符：

    ```bash
    exit
    ```

1. 在 Cloud Shell 中，从虚拟机复制架构文件。将 *nn.nn.nn.nn* 替换成虚拟机的 IP 地址。出现提示时，输入密码“**Pa55w.rdDemo**”：

    ```bash
    scp azureuser@nn.nn.nn.nn:~/adventureworks_mysql_schema.sql adventureworks_mysql_schema.sql
    ```
### 任务 2。创建 Azure Database for MySQL 服务器和数据库

1. 切换到 Azure 门户。

1. 单击 **“创建资源”**。

1. 在“**搜索商城**”方框中，键入“**Azure Database for MySQL**”，然后按 Enter。

1. 在“**Azure Database for MySQL**”页面上，单击“**创建**”。

1. 在“**创建 MySQL 服务器**”页面，输入以下详细信息，然后单击“**查看 + 创建**”：

    | 属性  | 值  |
    |---|---|
    | 资源组 | 使用你之前在本实验室的“*设置*”任务中创建 Azure 虚拟机时指定的同一个资源组 |
    | 服务器名称 | **adventureworks*nnn***，其中“*nnn*”是你选择的后缀，以使服务器名为唯一名称 |
    | 数据源 | 无 |
    | 管理员用户名 | awadmin |
    | 密码 | Pa55w.rdDemo |
    | 确认密码 | Pa55w.rdDemo |
    | 位置 | 选择离你最近的位置 |
    | 版本 | 5.7 |
    | 计算+ 存储 | 单击“**配置服务器**”，选择“**基本**”定价层，然后单击“**确定**” |

1. 在“**查看 + 创建**”页面，单击“**创建”**”。等待创建服务后再继续。

1. 创建服务后，请转到门户中的服务页面，然后单击“**连接安全性**”。


1. 在“**连接安全性**”页面，将“**允许访问 Azure 服务**”设为“**开**”。

1. 在防火墙规则列表中，添加名为“**VM**”的规则，然后将“**起始 IP 地址**”和“**结束 IP 地址**”设为运行虚拟机的 IP 地址，此虚拟机是执行 MySQL 服务器的虚拟机。

1. 单击“**添加客户端 IP**”，以使充当本地服务器的 **LON-DEV-01** 虚拟机连接到 Azure Database for MySQL。稍后，在运行重新配置的客户端应用程序时，你将需要此访问权限。

1. “**保存**”，然后等待防火墙规则更新。

1. 在 Cloud Shell 提示符下，运行以下命令以在 Azure Database for MySQL 服务中创建一个新数据库。将“*[nnn]*”替换成你在创建 Azure Database for MySQL 服务时使用的后缀。将“*resource group*”替换成你为服务指定的资源组的名称：

    ```bash
    az MySQL db create \
    --name azureadventureworks \
    --server-name adventureworks[nnn] \
    --resource-group [resource group]
    ```

    如果数据库创建成功，则应该看到类似于以下内容的消息：

    ```text
    {
          "charset": "latin1",
          "collation": "latin1_swedish_ci",
          "id": "/subscriptions/nnnnnnnnnnnnnnnnnnnnnnnnnnnnn/resourceGroups/nnnnnn/providers/Microsoft.DBforMySQL/servers/adventureworksnnnn/databases/azureadventureworks",
          "name": "azureadventureworks",
          "resourceGroup": "nnnnn",
          "type": "Microsoft.DBforMySQL/servers/databases"
    }
    ```

### 任务 3：将架构导入目标数据库

1. 在 Cloud Shell 中，运行以下命令以连接到 azureadventureworks[nnn] 服务器。将“*nnn*”的两个实例替换成服务的后缀。请注意，用户名后缀为“*adventureworks[nnn]*”。在密码提示下，输入“**Pa55w.rdDemo**”。

    ```bash
    mysql -h adventureworks[nnn].MySQL.database.azure.com -u awadmin@adventureworks[nnn] -pPa55w.rdDemo
    ```

1. 运行以下命令以创建一个名为“*azureuser*”的用户并将该用户的密码设置为“*Pa55w.rd*”。第二条语句为“*azureuser*”指定必要的特权，以在 *adventureworks[nnn]* 数据库中创建对象。

    ```SQL
    GRANT SELECT ON *.* TO 'azureuser'@'localhost' IDENTIFIED BY 'Pa55w.rd';
    GRANT CREATE ON *.* TO 'azureuser'@'localhost';
    ```

1. 运行以下命令以创建 *adventureworks* 数据库。

    ```SQL
    CREATE DATABASE adventureworks;
    ```

1. 使用 **quit** 命令关闭“*mysql*”实用程序。

1. 将 **adventureworks** 架构导入 Azure Database for MySQL 服务。你正在以 *azureuser* 的身份执行导入，因此在出现提示时，输入密码“**Pa55w.rd**”。

    ```bash
    mysql -h adventureworks[nnnn].MySQL.database.azure.com -u awadmin@adventureworks[nnn] -pPa55w.rdDemo adventureworks < adventureworks_mysql_schema.sql
    ```

### 任务 4。  使用数据库迁移服务执行联机迁移

1. 切换回 Azure 门户。

2. 单击“**所有服务**”，单击“**订阅**”，然后单击你的订阅。

3. 在你的订阅页面上，单击“**设置**”下方的“**资源提供程序**”。

4. 在“**按名称筛选**”方框中，键入“**DataMigration**”，然后单击“**Microsoft.DataMigration**”。

5. 如果没有注册“**Microsoft.DataMigration**”，请单击“**注册**”，然后等待“**状态**”更改为“**已注册**”。可能需要单击“**刷新**”查看状态更改。

6. 单击“**创建资源**”，在“**搜索商城**”方框中，键入“**Azure 数据库迁移服务**”，然后按 Enter。

7. 在“**Azure 数据库迁移服务**”屏幕上，单击“**创建**”。

8. 在“**创建迁移服务**”页面上，输入以下详细信息，然后单击“**创建**”。

    | 属性  | 值  |
    |---|---|
    | 服务名 | adventureworks_migration_service |
    | 订阅 | 选择你自己的订阅 |
    | 选择资源组 | 指定用于 Azure Database for MySQL 服务和 Azure 虚拟机的资源组 |
    | 位置 | 选择离你最近的位置 |
    | 虚拟网络 | 选择“**MySQLvnet/mysqlvmSubnet**”虚拟网络。此网络是在设置过程中创建的 |
    | 定价层 | 高级版，带有 4 个 vCore |

9. 等待创建数据库迁移服务。此过程可能需要几分钟时间。

10. 在 Azure 门户中，转到你的数据库迁移服务页面。

11. 单击“**新迁移项目**”。

12. 在“**新迁移项目**”页面，输入以下详细信息，然后单击“**创建并运行活动**”。

    | 属性  | 值  |
    |---|---|
    | 项目名 | adventureworks_migration_project |
    | 源服务器类型 | MySQL |
    | MySQL 的目标数据库 | 适用于 MySQL 的 Azure 数据库 |
    | 选择活动类型 | 联机数据迁移 |

13. 当“**迁移向导**”启动时，在“**添加源详细信息**”页面，输入以下详细信息，然后单击“**保存**”。

    | 属性  | 值  |
    |---|---|
    | 源服务器名 | nn.nn.nn.nn *（运行 MySQL 的 Azure 虚拟机的 IP 地址）* |
    | 服务器端口 | 3306 |
    | 用户名 | azureuser |
    | 密码 | Pa55w.rd |

14. 在“**目标详情**”页，输入以下详细信息，然后单击“**保存**”。

    | 属性  | 值  |
    |---|---|
    | 目标服务器名 | adventureworks[nnn].MySQL.database.azure.com |
    | 用户名 | awadmin@adventureworks[nnn] |
    | 密码 | Pa55w.rdDemo |

15. 在“**映射到目标数据库**”页面上，选择“**adventureworks**”数据库并将其映射到“**adventureworks**”。单击“**保存**”。

16. 在“**迁移设置**”页面，展开“**adventureworks**”下拉菜单，展开“**高级联机迁移设置**”下拉菜单，确认“**并行加载的实例数上限**”设置为 5，然后单击“**保存**”。

17. 在“**迁移摘要**”页面，在“**活动名**”方框中键入“**AdventureWorks_Migration_Activity**”，然后单击“**运行迁移**”。

18. 在“**AdventureWorks_Migration_Activity**”页面上，以 15 秒的间隔单击“**刷新**”。随着迁移操作的进行，你将看到其状态。等待直至“**迁移细节**”列更改为“**准备直接转换**”。


### 任务 5。修改数据，并直接转换到新数据库

1. 返回到 Azure 门户中的“**AdventureWorks_Migration_Activity**”页面。

1. 单击“**adventureworks**”数据库。

1. 在“**adventureworks**”页上，验证已将所有表的状态都标记为“**已完成**”。

1. 单击“**增量数据同步**”。验证已将所有表的状态都标记为“**同步**”。

1. 切换回 Cloud Shell。

1. 运行以下命令以连接到 **adventureworks** 数据库，此数据库使用 MySQL 在虚拟机上运行：

    ```bash
    mysql -h nn.nn.nn.nn -u azureuser -pPa55w.rd adventureworks
    ```

1. 执行以下 SQL 语句以显示订单 43659、43660 和 43661，然后从数据库中将其删除。请注意，数据库在 *salesorderheader* 表上实现了级联删除，该表会自动从 *salesorderdetail* 表中删除相应的行。

    ```SQL
    SELECT * FROM salesorderheader WHERE salesorderid IN (43659, 43660, 43661);
    SELECT * FROM salesorderdetail WHERE salesorderid IN (43659, 43660, 43661);
    DELETE FROM salesorderheader WHERE salesorderid IN (43659, 43660, 43661);
    ```

1. 使用 **quit** 命令关闭“*mysql*”实用程序。

2. 返回到 Azure 门户中的“**adventureworks**”页面，然后单击“**刷新**”。滚动到“*salesorderheader*”和“*salesorderdetail*”表的页面。验证 *salesorderheader* 表显示已删除 3 行，并从 **sales.salesorderdetail** 表中删除了 29 行。如果没有应用更新，请检查是否有数据库的 **挂起的更改**。

1. 单击“**开始直接转换**”。

1. 在“**完成直接转换**”页面，选择“**确认**”，然后单击“**应用**”。等待状态更改为“**已完成**”。

1. 返回 Cloud Shell。

1. 运行以下命令以连接到 **azureadventureworks** 数据库，该数据库使用 Azure Database for MySQL 服务运行：

    ```bash
    mysql -h adventureworks[nnn].MySQL.database.azure.com -u awadmin@adventureworks[nnn] -pPa55w.rdDemo adventureworks
    ```

1. 运行以下 SQL 语句以显示订单和订单 43659、43660 和 43661 的详细信息。这些查询的目的是为了表明数据已被传输：

    ```SQL
    SELECT * FROM salesorderheader WHERE salesorderid IN (43659, 43660, 43661);
    SELECT * FROM salesorderdetail WHERE salesorderid IN (43659, 43660, 43661);
    ```

    第一个查询应返回 3 行。第二个查询应返回 29 行。

1. 使用 **quit** 命令关闭“*mysql*”实用程序。

### 任务 6：验证 Azure Database for MySQL 中的数据库

1. 返回到充当本地计算机的虚拟机

1. 切换到“**MySQL Workbench**”工具。
1. 在“**数据库**”菜单上，单击“**连接数据库**”

1. 输入以下详细信息，然后单击“**确定**”：

    | 属性  | 值  |
    |---|---|
    | 主机名 | adventureworks*[nnn]*.MySQL.database.azure.com |
    | 端口 | 3306 |
    | 用户名 | awadmin@adventureworks*[nnn]* |
    | 密码 | Pa55w.rdDemo |

1. 展开“**数据库**”，展开“**adventureworks**”，然后浏览数据库中的表。这些表应与本地数据库中的表相同。

### 任务 7：根据 Azure Database for MySQL 中的数据库重新配置和测试示例应用程序

1. 返回到**LON-DEV-01**虚拟机上的**终端**窗口。
1. 移动到**workshop/migration_samples/code/mysql/AdventureWorksQueries**文件夹。

   ```bash
   cd ~/workshop/migration_samples/code/mysql/AdventureWorksQueries
   ```

1. 使用 nano 编辑器打开 App.config 文件：

    ```bash
    nano App.config
    ```

1. 更改“**ConnectionString**”设置的值，将 Azure 虚拟机的 IP 地址替换为“**adventureworks[nnn].MySQL.database.azure.com**”。将“**用户 ID**”更改为“**awadmin@adventureworks[nnn]**”。将“**密码**”更改为“**Pa55w.rdDemo**”。文件内容应如下：

    ```XML
    <?xml version="1.0" encoding="utf-8" ?>
        <configuration>
          <appSettings>
            <add key="ConnectionString" value="Server=adventureworks[nnn].MySQL.database.azure.com;database=adventureworks;port=3306;uid=awadmin@adventureworks[nnn];password=Pa55w.rdDemo" />
          </appSettings>
        </configuration>
    ```

    应用程序现在应连接到在 Azure Database for MySQL 上运行的数据库。

1. 保存文件并关闭编辑器。

1. 生成并运行该应用程序：

    ```bash
    dotnet run
    ```

   应用应显示与往常相同的结果，区别在于，应用现在要从 Azure 中运行的数据库中检索数据。
   现在，你已将数据库迁移到 Azure Database for MySQL，并重新配置了应用程序以使用新数据库。
