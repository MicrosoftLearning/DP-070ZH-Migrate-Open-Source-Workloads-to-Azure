---
lab:
    title: '将本地 PostgreSQL 数据库迁移至 Azure'
    module: '模块 3：将本地 PostgreSQL 迁移至 Azure Database for PostgreSQL'
---

# 实验室：将本地 PostgreSQL 数据库迁移至 Azure

## 概述

在本实验室中，您将使用在本模块中学习到的信息将 PostgreSQL 数据库迁移到 Azure。为了完全涵盖所学内容，学生将执行两次迁移。第一个是脱机迁移，将本地 PostgreSQL 数据库传输到在 Azure 上运行的虚拟机。第二个是将虚拟机上运行的数据库联机迁移到 Azure Database for PostgreSQL。

你还将重新配置并运行一个使用数据库的示例应用程序，以验证每次迁移后数据库是否正常运行。

## 学习目标

完成本实验室后，你将能够：

1. 执行本地 PostgreSQL 数据库到 Azure 虚拟机的脱机迁移。

1. 将在虚拟机上运行的 PostgreSQL 数据库联机迁移到 Azure Database for PostgreSQL。

## 场景

你是 AdventureWorks 组织的数据库开发人员。十多年来，AdventureWorks 一直将自行车和自行车零件直接销售给最终用户和分销商。他们的系统将信息存储在当前使用 PostgreSQL 运行的数据库中，该数据库位于其本地数据中心中。作为硬件合理化练习的一部分，AdventureWorks 希望将数据库移至 Azure。已要求你执行此迁移。

最初，你决定将数据快速重定位到在 Azure 虚拟机上运行的 PostgreSQL 数据库。这被视为一种低风险的方法，因为它几乎不需要对数据库进行任何更改。但是，此方法确实需要你继续执行与数据库相关的大多数日常监视和管理任务。你还需要考虑 AdventureWorks 的客户群如何变化。最初，AdventureWorks 的目标客户是本地区的客户，但如今，他们已扩展到全球范围。客户可以位于任何地方，而确保查询数据库的客户延迟时间最短是首要问题。你可以对位于其他区域的虚拟机实施 PostgreSQL 复制，但这同样是管理上的开销。

相反，一旦系统在虚拟机上运行，你就可以考虑采用更长远的解决方案；你将数据库迁移到 Azure Database for PostgreSQL。此 PaaS 策略省去了与维护系统相关的大量工作。你可以轻松扩展系统，并添加只读副本以支持全球任何地方的客户。此外，Microsoft 提供了有保证的 SLA 可用性。

## 设置

你有一个内部环境，其中一个现有的 PostgreSQL 数据库包含要迁移到 Azure 的数据。在开始实验室之前，你需要创建一个运行 PostgreSQL 的 Azure 虚拟机，该虚拟机将充当初始迁移的目标。我们提供了一个脚本，用于创建该虚拟机并进行配置。使用以下步骤下载并运行此脚本：

1. 登录到在教室环境中运行的 **LON-DEV-01** 虚拟机。用户名是“**azureuser**”，密码为“**Pa55w.rd**”。

2. 使用浏览器登录至 Azure 门户。

3. 打开“Azure Cloud Shell”窗口。确保你正在运行 **Bash** shell。

4. 如果以前未执行此操作，请克隆包含脚本和示例数据库的存储库。

    ```bash
    git clone https://github.com/MicrosoftLearning/DP-070-Migrate-Open-Source-Workloads-to-Azure ~/workshop
    ```

5. 移动到“*workshop/migration_samples/setup*”文件夹。

    ```bash
    cd ~/workshop/migration_samples/setup
    ```

6. 按如下方式运行“*create_postgresql_vm.sh*”开脚本。指定资源组的名称和用于以参数方式承载虚拟机的位置。如果资源组尚不存在，则会创建该资源组。指定你附近的位置，例如“*eastus*”或“*uksouth*”：

    ```bash
    bash create_postgresql_vm.sh [resource group name] [location]
    ```

    该脚本大约需要 10 分钟的运行时间。它将在运行时生成大量输出，最后输出新虚拟机的 IP 地址和“**设定完成**”消息。记下该 IP 地址。
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

1. 登录到在教室环境中运行的 **LON-DEV-01** 虚拟机。用户名是“**azureuser**”，密码为“**Pa55w.rd**”。

    该虚拟机模拟你的本地环境。它正在运行 PostgreSQL 服务器，该服务器承载着你需要迁移的 AdventureWorks 数据库。

2. 在教室环境中运行的 **LON-DEV-01** 虚拟机上，在在屏幕左侧的“**收藏夹**”工具栏中，单击“**pgAdmin4**”实用程序。

3. 在“**解锁保存的密码**”对话框中输入密码“**Pa55w.rd**”，然后单击“**确认**”。

4. 在“**pgAdmin4**”窗口，展开“**服务器**”，展开“**LON-DEV-01**”，展开“**数据库**”，展开“**adventureworks**”，然后展开“**架构**”。

5. 在“**销售**”架构中，展开“**表**”。

6. 右击“**salesorderheader**”表，单击“**脚本**”，然后单击“**选择脚本**”。

7. 按 **F5** 运行查询。它应该返回 31465 行。

8. 在表列表中，右击“**salesorderdetail**”表，单击“**脚本**”，然后单击“**选择脚本**”。

9. 按 **F5** 运行查询。它应该返回 121317 行。

10. 花几分钟时间浏览数据库内各种架构中其他表的数据。

### 任务 2：运行查询数据库的示例应用程序

1. 在 **LON-DEV-01** 虚拟机的在收藏夹栏上，单击“**终端**”打开终端窗口。

1. 在终端窗口中，下载示例代码进行演示。如果出现提示，请在密码中输入“**Pa55w.rd**”：

   ```bash
   sudo rm -rf ~/workshop
   git clone  https://github.com/MicrosoftLearning/DP-070-Migrate-Open-Source-Workloads-to-Azure ~/workshop
   ```

1. 移动到“*~/workshop/migration_samples/code/postgresql/AdventureWorksQueries*”文件夹：

   ```bash
   cd ~/workshop/migration_samples/code/postgresql/AdventureWorksQueries
   ```

    此文件夹包含一个示例应用程序，该应用程序运行查询以计算 *adventureworks* 数据库内几个表中的行数。

1. 运行应用程序：

    ```bash
    dotnet run
    ```

    该应用程序应生成以下输出：

    ```text
    Querying AdventureWorks database
    SELECT COUNT(*) FROM production.vproductanddescription
    1764

    SELECT COUNT(*) FROM purchasing.vendor
    104

    SELECT COUNT(*) FROM sales.specialoffer
    16

    SELECT COUNT(*) FROM sales.salesorderheader
    31465

    SELECT COUNT(*) FROM sales.salesorderdetail
    121317

    SELECT COUNT(*) FROM person.person
    19972
    ```

### 任务 3：执行数据库到 Azure 虚拟机的脱机迁移

现在，你已经了解了 adventureworks 数据库中的数据，可以将其迁移到在 Azure 内的虚拟机上运行的 Postgresql 服务器。你将使用备份和还原命令将此操作作为脱机任务执行。

> [!注意]
> 如果要联机迁移数据，则可以配置从本地数据库复制到在 Azure 虚拟机上运行的数据库。

1. 在终端窗口中，运行以下命令以备份 *adventureworks* 数据库：

    ```bash
    pg_dump adventureworks -U azureuser -Fc > adventureworks_backup.bak
    ```

1. 在 Azure 虚拟机上创建目标数据库。将 [nn.nn.nn.nn] 替换为虚拟机的 IP 地址，该虚拟机即本实验室设置阶段创建的虚拟机。当提示你输入“**azureuser**”的密码时，输入密码“**Pa55w.rd**”：

    ```azurecli
    createdb -h [nn.nn.nn.nn] -U azureuser --password adventureworks
    ```

1. 使用 pg_restore 命令将备份还原到新数据库中：

    ```bash
    pg_restore -d adventureworks -h [nn.nn.nn.nn] -Fc -U azureuser --password adventureworks_backup.bak
    ```

    执行此命令可能需要几分钟。

### 任务 4：验证 Azure 虚拟机上的数据库

1. 运行以下命令以连接到 Azure 虚拟机上的数据库。对于在虚拟机上运行的 PostgreSQL 服务器中的“*azureuser*”用户，其密码是“**Pa55w.rd**”：

    ```bash
    psql -h [nn.nn.nn.nn] -U azureuser adventureworks
    ```

1. 运行以下查询：

    ```SQL
    SELECT COUNT(*) FROM sales.salesorderheader;
    ```

    验证此查询返回 31465 行。这与本地数据库中的行数相同。

1. 查询 *sales.salesorderdetail* 表中的行数。

    ```SQL
    SELECT COUNT(*) FROM sales.salesorderdetail;
    ```

    该表应包含 121317 行。

1. 使用 **\q** 命令关闭“*psql*”实用程序。

1. 切换到 **pgAdmin4** 工具。

1. 在左窗格中，右击“**服务器**”，单击“**创建**”，然后单击“**服务器**”。

1. 在“**创建 - 服务器**”对话框中的“**常规**”选项卡上，在“**名称**”方框中输入“**Virtual Machine**”，然后单击“**连接**”选项卡。

1. 输入以下详细信息，然后单击“**保存**”：

    | 属性  | 值  |
    |---|---|
    | 主机名/地址 | *[nn.nn.nn.nn]* |
    | 端口 | 5432 |
    | 维护资料库 | postgres |
    | 用户名 | azureuser |
    | 密码 | Pa55w.rd |
    | 保存密码 | Select |
    | 角色 | *保留空白* |
    | 服务 | *保留空白* |

1. 在“**pgAdmin4**”窗口的左窗格中，在“**服务器**”下展开“**虚拟机**”。

1. 展开“**数据库**”，展开“**adventureworks**”，然后浏览数据库中的架构和表。这些表应与本地数据库中的表相同。

### 任务 5：根据 Azure 虚拟机上的数据库重新配置和测试示例应用程序

1. 返回到“**终端**”窗口。

1. 使用“*nano*”编辑器打开测试应用程序的 App.config 文件。

    ```bash
    nano App.config
    ```

1. 更改“**ConnectionString**”设置的值，将“**localhost**”替换为 Azure 虚拟机的 IP 地址。文件内容应如下：

    ```XML
    <?xml version="1.0" encoding="utf-8" ?>
    <configuration>
      <appSettings>
        <add key="ConnectionString" value="Server=nn.nn.nn.nn;Database=adventureworks;Port=5432;User Id=azureuser;Password=Pa55w.rd;" />
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

## 练习 2：执行到 Azure Database for PostgreSQL 的联机迁移

在该练习中，你将执行以下任务：

1. 配置在 Azure 虚拟机上运行的 PostgreSQL 服务器并导出架构。
2. 创建 Azure Database for PostgreSQL 服务器和数据库。
3. 将架构导入目标数据库。
4. 使用数据库迁移服务执行联机迁移
5. 修改数据，并直接转换到新数据库
6. 验证 Azure Database for PostgreSQL 中的数据库
7. 根据 Azure Database for PostgreSQL 中的数据库重新配置和测试示例应用程序。

### 任务 1：配置在 Azure 虚拟机上运行的 PostgreSQL 服务器并导出架构

1. 使用 Web 浏览器，返回到 Azure 门户。

1. 打开“Azure Cloud Shell”窗口。确保你正在运行 **Bash** shell。

1. 如果以前未执行此操作，请克隆包含脚本和示例数据库的存储库。

    ```bash
    git clone https://github.com/MicrosoftLearning/DP-070-Migrate-Open-Source-Workloads-to-Azure ~/workshop
    ```

1. 移动到“*~/workshop/migration_samples/setup/postgresql/adventureworks*”文件夹。

    ```bash
    cd ~/workshop/migration_samples/setup/postgresql/adventureworks

1. 连接到运行 PostgreSQL 服务器的 Azure 虚拟机。在以下命令中，将 *nn.nn.nn.nn* 替换成虚拟机的 IP 地址。出现提示时，输入密码“**Pa55w.rdDemo**”：

    ```bash
    ssh azureuser@nn.nn.nn.nn
    ```

1. 在虚拟机上，切换到“*根*”帐户。如果出现提示，则输入“*azureuser*”用户的密码 (**Pa55w.rdDemo**)。

    ```bash
    sudo bash
    ```

1. 移动到目录“*/etc/postgresql/10/main*”：

    ```bash
    cd /etc/postgresql/10/main
    ```

1. 使用“*nano*”编辑器，打开“*postgresql.conf*”文件。

    ```bash
    nano postgresql.conf
    ```

1. 滚动到文件底部，并验证是否已配置以下参数：

    ```text
    listen-addresses = '*'
    wal_level = logical
    max_replication_slots = 5
    max_wal_senders = 10
    ```

1. 要保存文件并关闭编辑器，请按 ESC，然后按 CTRL+X。如果出现提示，请按 Enter 保存更改。

1. 重新启动 PostgreSQL 服务：

    ```bash
    service postgresql restart
    ```

1. 验证 Postgres 已正确启动：

    ```bash
    service postgresql status
    ```

    如果该服务正在运行，则应该看到类似于以下内容的消息：

    ```text
     postgresql.service - PostgreSQL RDBMS
      Loaded: loaded (/lib/systemd/system/postgresql.service; enabled; vendor preset: enabled)
       Active: active (exited) since Fri 2019-08-23 12:47:02 UTC; 2min 3s ago
       Process: 115562 ExecStart=/bin/true (code=exited, status=0/SUCCESS)
      Main PID: 115562 (code=exited, status=0/SUCCESS)

    Aug 23 12:47:02 postgreSQLVM systemd[1]: Starting PostgreSQL RDBMS...
    Aug 23 12:47:02 postgreSQLVM systemd[1]: Started PostgreSQL RDBMS.
    ```

1. 离开“*根*”帐户并返回“*azureuser*”帐户：

    ```bash
    exit
    ```

1. 在 bash 提示符下，运行以下命令以将 **adventureworks** 的架构导出到名为“**adventureworks_schema.sql**”的文件

    ```bash
    pg_dump -o  -d adventureworks -s > adventureworks_schema.sql
    ```

1. 断开与虚拟机的连接并返回到 Cloud Shell 提示符：

    ```bash
    exit
    ```

1. 在 Cloud Shell 中，从虚拟机复制架构文件。将 *nn.nn.nn.nn* 替换成虚拟机的 IP 地址。出现提示时，输入密码“**Pa55w.rdDemo**”：

    ```bash
    scp azureuser@nn.nn.nn.nn:~/adventureworks_schema.sql adventureworks_schema.sql
    ```

## 任务 2。创建 Azure Database for PostgreSQL 服务器和数据库

1. 切换到 Azure 门户。

2. 单击“**+ 创建资源**”。

3. 在“**搜索商城**”方框中，键入“**Azure Database for PostgreSQL**”，然后按 Enter。

4. 在“**Azure Database for PostgreSQL**”页面上，单击“**创建**”。

5. 在“**选择 Azure Database for PostgreSQL 部署选项**”页面，在“**单一服务器**”方框中单击“**创建**”。

6. 在“**单一服务器**”页面，输入以下详细信息，然后单击“**查看 + 创建**”：

    | 属性  | 值  |
    |---|---|
    | 资源组 | 使用你之前在本实验室的“*设置*”任务中创建 Azure 虚拟机时指定的同一个资源组 |
    | 服务器名称 | **adventureworks*nnn***，其中“*nnn*”是你选择的后缀，以使服务器名为唯一名称 |
    | 数据源 | 无 |
    | 管理员用户名 | awadmin |
    | 密码 | Pa55w.rdDemo |
    | 确认密码 | Pa55w.rdDemo |
    | 位置 | 选择离你最近的位置 |
    | 版本 | 10 |
    | 计算+ 存储 | 单击“**配置服务器**”，选择“**基本**”定价层，然后单击“**确定**” |

7. 在“**查看 + 创建**”页面，单击“**创建”**”。等待创建服务后再继续。

8. 创建服务后，请转到门户中的服务页面，然后单击“**连接安全性**”。

9. 在“**连接安全性**”页面，将“**允许访问 Azure 服务**”设为“**开**”。

10. 在防火墙规则列表中，添加名为“**VM**”的规则，然后将“**起始 IP 地址**”和“**结束 IP 地址**”设为虚拟机的 IP 地址，此虚拟机是执行你之前创建的 PostgreSQL 服务器的虚拟机。

11. 单击“**添加客户端 IP**”，以使充当本地服务器的 **LON-DEV-01** 虚拟机连接到 Azure Database for PostgreSQL。稍后，在运行重新配置的客户端应用程序时，你将需要此访问权限。

12. “**保存**”，然后等待防火墙规则更新。

13. 在 Cloud Shell 提示符下，运行以下命令以在 Azure Database for PostgreSQL 服务中创建一个新数据库。将“*[nnn]*”替换成你在创建 Azure Database for PostgreSQL 服务时使用的后缀。将“*[resource group]*”替换成你为服务指定的资源组的名称：

    ```bash
    az postgres db create \
      --name azureadventureworks \
      --server-name adventureworks[nnn] \
      --resource-group [resource group]
    ```

    如果数据库创建成功，则应该看到类似于以下内容的消息：

    ```text
    {
      "charset": "UTF8",
      "collation": "English_United States.1252",
      "id": "/subscriptions/nnnnnnnnnnnnnnnnnnnnnn/resourceGroups/nnnnnnnn/providers/Microsoft.DBforPostgreSQL/servers/adventureworksnnn/databases/azureadventureworks",
      "name": "azureadventureworks",
      "resourceGroup": "nnnnnnnn",
      "type": "Microsoft.DBforPostgreSQL/servers/databases"
    }
    ```

### 任务 3：将架构导入目标数据库

1. 在 Cloud Shell 中，运行以下命令以连接到 azureadventureworks[nnn] 服务器。将“*[nnn]*”的两个实例替换成服务的后缀。请注意，用户名后缀为“*@adventureworks[nnn]*”。在密码提示下，输入“**Pa55w.rdDemo**”。

    ```bash
    psql -h adventureworks[nnn].postgres.database.azure.com -U awadmin@adventureworks[nnn] -d postgres
    ```

2. 运行以下命令以创建一个名为“*azureuser*”的用户并将该用户的密码设置为“*Pa55w.rd*”。第三条语句为“*azureuser*”指定必要的特权，以在 *azureadventureworks* 数据库中创建和管理对象。“*azure_pg_admin*”角色使“*azureuser*”用户能够在数据库中安装和使用扩展。

    ```SQL
    CREATE ROLE azureuser WITH LOGIN;
    ALTER ROLE azureuser PASSWORD 'Pa55w.rd';
    GRANT ALL PRIVILEGES ON DATABASE azureadventureworks TO azureuser;
    GRANT azure_pg_admin TO azureuser;
    ```

3. 使用 **\q** 命令关闭“*psql*”实用程序。

4. 将 **adventureworks** 数据库的架构导入到在 Azure Database for PostgreSQL 服务上运行的 **azureadventureworks** 数据库。你正在以 *azureuser* 的身份执行导入，因此在出现提示时，输入密码“**Pa55w.rd**”。

    ```bash
    psql -h adventureworks[nnn].postgres.database.azure.com -U azureuser@adventureworks[nnn] -d azureadventureworks -f adventureworks_schema.sql
    ```

    创建每个项目时，你将看到一系列消息。该脚本应该在没有任何错误的情况下完成。

5. 执行以下命令。*findkeys.sql* 脚本生成另一个名为 *dropkeys.sql* 的 SQL 脚本，此脚本将从 **azureadventureworks** 数据库的表中删除所有外键。你将短暂地运行 *dropkeys.sql* 脚本：

    ```bash
    psql -h adventureworks[nnn].postgres.database.azure.com -U azureuser@adventureworks[nnn] -d azureadventureworks -f findkeys.sql -o dropkeys.sql -t
    ```

    如果有时间，你可以使用文本编辑器检查 *dropkeys.sql* 脚本。

6. 执行以下命令。*createkeys.sql* 脚本生成另一个名为 *addkeys.sql* 的 SQL 脚本，此脚本将重新创建所有外键。在千亿数据库后，你将运行 *addkeys.sql* 脚本：

    ```bash
    psql -h adventureworks[nnn].postgres.database.azure.com -U azureuser@adventureworks[nnn] -d azureadventureworks -f createkeys.sql -o addkeys.sql -t
    ```

7. 运行 *dropkeys.sql* 脚本：

    ```bash
    psql -h adventureworks[nnn].postgres.database.azure.com -U azureuser@adventureworks[nnn] -d azureadventureworks -f dropkeys.sql
    ```

    在删除外键时，你会看到显示一系列“**ALTER TABLE**”消息。

8. 重新统计 psql 实用程序并连接到 *azureadventureworks* 数据库。

    ```bash
    psql -h adventureworks[nnn].postgres.database.azure.com -U azureuser@adventureworks[nnn] -d azureadventureworks
    ```

9. 运行以下查询以查找所有剩余外键的详细信息：

    ```SQL
    SELECT constraint_type, table_schema, table_name, constraint_name
    FROM information_schema.table_constraints
    WHERE constraint_type = 'FOREIGN KEY';
    ```

    该查询应返回空结果集。但是，如果仍然存在任何外键，请为每个外键运行以下命令：

    ```SQL
    ALTER TABLE [table_schema].[table_name] DROP CONSTRAINT [constraint_name];
    ```

10. 删除所有剩余的外键之后，执行以下 SQL 语句以在数据库中显示触发器：

    ```bash
    SELECT trigger_name
    FROM information_schema.triggers;
    ```

    该查询还应返回一个空结果集，表明数据库不包含任何触发器。如果数据库确实包含触发器，则必须在迁移数据之前将其禁用，迁移之后再重新启用它们。

11. 使用 **\q** 命令关闭“*psql*”实用程序。

## 任务 4。  使用数据库迁移服务执行联机迁移

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
    | 选择资源组 | 指定用于 Azure Database for PostgreSQL 服务和 Azure 虚拟机的资源组 |
    | 位置 | 选择离你最近的位置 |
    | 虚拟网络 | 选择“**postgresqlvnet / posgresqlvmSubnet**”虚拟网络。此网络是在设置过程中创建的 |
    | 定价层 | 高级版，带有 4 个 vCore |

9. 等待创建数据库迁移服务。此过程可能需要几分钟时间。

10. 在 Azure 门户中，转到你的数据库迁移服务页面。

11. 单击“**新迁移项目**”。

12. 在“**新迁移项目**”页面，输入以下详细信息，然后单击“**创建并运行活动**”。

    | 属性  | 值  |
    |---|---|
    | 项目名 | adventureworks_migration_project |
    | 源服务器类型 | PostgreSQL |
    | PostgreSQL 的目标数据库 | Azure Database for PostgreSQL |
    | 选择活动类型 | 联机数据迁移 |

13. 当“**迁移向导**”启动时，在“**添加源详细信息**”页面，输入以下详细信息，然后单击“**保存**”。

    | 属性  | 值  |
    |---|---|
    | 源服务器名 | nn.nn.nn.nn *（运行 PostgreSQL 的 Azure 虚拟机的 IP 地址）* |
    | 服务器端口 | 5432 |
    | 数据库 | AdventureWorks |
    | 用户名 | azureuser |
    | 密码 | Pa55w.rd |

14. 在“**目标详情**”页，输入以下详细信息，然后单击“**保存**”。

    | 属性  | 值  |
    |---|---|
    | 目标服务器名 | adventureworks[nnn].postgres.database.azure.com |
    | 数据库 | azureadventureworks |
    | 用户名 | azureuser@adventureworks[nnn] |
    | 密码 | Pa55w.rd |

15. 在“**映射到目标数据库**”页面上，选择“**adventureworks**”数据库并将其映射到“**azureadventureworks**”。取消选择“**postgres**”数据库。单击“**保存**”。

16. 在“**迁移设置**”页面，展开“**adventureworks**”下拉菜单，展开“**高级联机迁移设置**”下拉菜单，确认“**并行加载的实例数上限**”设置为 5，然后单击“**保存**”。

17. 在“**迁移摘要**”页面，在“**活动名**”方框中键入“**AdventureWorks_Migration_Activity**”，然后单击“**运行迁移**”。

18. 在“**AdventureWorks_Migration_Activity**”页面上，以 15 秒的间隔单击“**刷新**”。随着迁移操作的进行，你将看到其状态。等待直至“**迁移细节**”列更改为“**准备直接转换**”。

19. 切换回 Cloud Shell。

20. 运行以下命令以重新在 **azureadventureworks** 数据库中创建外键。你之前生成了 **addkeys.sql** 脚本：

    ```bash
    psql -h adventureworks[nnn].postgres.database.azure.com -U azureuser@adventureworks[nnn] -d azureadventureworks -f addkeys.sql
    ```

    你会看到以外键形式添加一系列“**ALTER TABLE**”语句。你可能会看到与 *SpecialOfferProduct* 表有关的错误，但现在可以忽略它。这是由于无法正确传输 UNIQUE 约束引起的。在现实世界中，应该使用以下查询从源数据库中检索此约束的详细信息：

    ```SQL
    SELECT constraint_type, table_schema, table_name, constraint_name
    FROM information_schema.table_constraints
    WHERE constraint_type = 'UNIQUE';
    ```

    然后，你可以在 Azure Database for PostgreSQL 的目标数据库中手动恢复此约束。

    不应该有其他错误。

## 任务 5。修改数据，并直接转换到新数据库

1. 返回到 Azure 门户中的“**AdventureWorks_Migration_Activity**”页面。

1. 单击“**adventureworks**”数据库。

1. 在“**adventureworks**”页上，验证已将所有表的状态都标记为“**已完成**”。

1. 单击“**增量数据同步**”。验证已将所有表的状态都标记为“**同步**”。

1. 切换回 Cloud Shell。

1. 运行以下命令以连接到 **adventureworks** 数据库，此数据库使用 PostgreSQL 在虚拟机上运行：

    ```bash
    psql -h nn.nn.nn.nn -U azureuser -d adventureworks
    ```

1. 执行以下 SQL 语句以显示订单 43659、43660 和 43661，然后从数据库中将其删除。请注意，数据库在 *salesorderheader* 表上实现了级联删除，该表会自动从 *salesorderdetail* 表中删除相应的行。

    ```SQL
    SELECT * FROM sales.salesorderheader WHERE salesorderid IN (43659, 43660, 43661);
    SELECT * FROM sales.salesorderdetail WHERE salesorderid IN (43659, 43660, 43661);
    DELETE FROM sales.salesorderheader WHERE salesorderid IN (43659, 43660, 43661);
    ```

1. 使用 **\q** 命令关闭“*psql*”实用程序。

1. 返回到 Azure 门户中的“**adventureworks**”页面，然后滚动到“*salesorderheader*”和“*salesorderdetail*”表的页面。单击“**刷新**”。验证 *salesorderheader* 表显示已删除 3 行，并从 **sales.salesorderdetail** 表中删除了 29 行。

1. 单击“**开始直接转换**”。

1. 在“**完成直接转换**”页面，选择“**确认**”，然后单击“**应用**”。等待状态更改为“**已完成**”。

1. 返回 Cloud Shell。

1. 运行以下命令以连接到 **azureadventureworks** 数据库，该数据库使用 Azure Database for PostgreSQL 服务运行：

    ```bash
    psql -h adventureworks[nnn].postgres.database.azure.com -U azureuser@adventureworks[nnn] -d azureadventureworks
    ```

1. 执行以下 SQL 语句以显示数据库中的订单和订单详细信息。在每个表的第一页之后退出。这些查询的目的是为了表明数据已被传输：

    ```SQL
    SELECT * FROM sales.salesorderheader;
    SELECT * FROM sales.salesorderdetail;
    ```

1. 运行以下 SQL 语句以显示订单和订单 43659、43660 和 43661 的详细信息。

    ```SQL
    SELECT * FROM sales.salesorderheader WHERE salesorderid IN (43659, 43660, 43661);
    SELECT * FROM sales.salesorderdetail WHERE salesorderid IN (43659, 43660, 43661);
    ```

    两个查询都应返回 0 行。

1. 使用 **\q** 命令关闭“*psql*”实用程序。

### 任务 6：验证 Azure Database for PostgreSQL 中的数据库

1. 返回到充当本地计算机的虚拟机

1. 切换到 **pgAdmin4** 工具。

1. 在左窗格中，右击“**服务器**”，单击“**创建**”，然后单击“**服务器**”。

1. 在“**创建 - 服务器**”对话框中的“**常规**”选项卡上，在“**名称**”方框中输入“**Azure Database for PostgreSQL**”，然后单击“**连接**”选项卡。

1. 输入以下详细信息，然后单击“**保存**”选项卡：

    | 属性  | 值  |
    |---|---|
    | 主机名/地址 | adventureworks*[nnn]*.postgres.database.azure.com |
    | 端口 | 5432 |
    | 维护资料库 | postgres |
    | 用户名 | azureuser@adventureworks*[nnn]* |
    | 密码 | Pa55w.rd |
    | 保存密码 | Select |
    | 角色 | *保留空白* |
    | 服务 | *保留空白* |

1. 在“**SSL**”选项卡上，将“**SSL 模式**”模式设为“**必需**”，然后单击“**保存**”。

1. 在“**pgAdmin4**”窗口的左窗格中，在“**服务器**”下展开“**Azure Database for PostgreSQL**”。

1. 展开“**数据库**”，展开“**azureadventureworks**”，然后浏览数据库中的架构和表。这些表应与本地数据库中的表相同。

### 任务 7：根据 Azure Database for PostgreSQL 中的数据库重新配置和测试示例应用程序

1. 返回到“**终端**”窗口。

2. 使用“*nano*”编辑器打开测试应用程序的 App.config 文件。

    ```bash
    nano App.config
    ```

3. 更改“**ConnectionString**”设置的值，将 Azure 虚拟机的 IP 地址替换为“**adventureworks[nnn].postgres.database.azure.com**”。  将“**数据库**”更改为“**azureadventureworks**”。将“**用户 ID**”更改为“**azureuser@adventureworks[nnn]**”。在连接字符串的末尾附加文字“**Ssl Mode=Require;**”。文件内容应如下：

    ```XML
    <?xml version="1.0" encoding="utf-8" ?>
    <configuration>
      <appSettings>
        <add key="ConnectionString" value="Server=adventureworks[nnn].postgres.database.azure.com;Database=azureadventureworks;Port=5432;User Id=azureuser@adventureworks[nnn];Password=Pa55w.rd;Ssl Mode=Require;" />
      </appSettings>
    </configuration>
    ```

    应用程序现在应连接到在 Azure 虚拟机上运行的数据库。

4. 要保存文件并关闭编辑器，请按 ESC，然后按 CTRL+X。出现提示时，请按 Enter 保存更改。

5. 生成并运行该应用程序：

    ```bash
    dotnet run
    ```

    该应用程序将连接到数据库，但会失败并显示以下消息“**materialized view "vproductanddescription” has not been populated**”（尚未填充实例化视图 "vproductanddescription"）。你需要刷新数据库中的实例化视图。

6. 在终端窗口中，使用 *psql* 实用程序以连接到 azureadventureworks 数据库：

    ```bash
    psql -h adventureworks[nnn].postgres.database.azure.com -U azureuser@adventureworks[nnn] -d azureadventureworks
    ```

7. 运行以下查询以列出数据库中的所有实例化视图：

    ```SQL
    SELECT schemaname, matviewname, matviewowner, ispopulated
    FROM pg_matviews;
    ```

    该查询应返回两行，*person.vstateprovincecountryregion* 和 *production.vproductanddescription*。两个视图的“*ispopulated*”列是“*f*”，表示尚未填充它们。

8. 运行以下语句以填充两个视图：

   ```SQL
   REFRESH MATERIALIZED VIEW person.vstateprovincecountryregion;
   REFRESH MATERIALIZED VIEW production.vproductanddescription;
   ```

9.  使用 **\q** 命令关闭“*psql*”实用程序。

10. 重新运行该应用程序：

    ```bash
    dotnet run
    ```

    验证应用程序现在运行成功。

    现在，你已将数据库迁移到 Azure Database for PostgreSQL，并重新配置了应用程序以使用新数据库。
