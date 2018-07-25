﻿using Eternet.Mikrotik;
using Eternet.Mikrotik.Entities;
using Eternet.Mikrotik.Entities.Interface;
using Eternet.Mikrotik.Entities.Interface.Ethernet.Poe;
using Eternet.Mikrotik.Entities.Ip;
using Eternet.Mikrotik.Entities.ReadWriters;
using Serilog;
using System.Collections.Generic;
using System.Linq;

namespace Pole.Tester
{
    public class PoleTester
    {
        public static ILogger Logger;

        public PoleTester(ILogger logger)
        {
            Logger = logger;
        }

        public List<(string, string)> GetNeighborsOnRunningInterfaces(IEntityReader<InterfaceEthernet> ethReader, IEntityReader<IpNeighbor> neigReader)
        {
            var interfacesToTest = new List<(string, string)>();

            var runningethers = ethReader.GetAll().Where(p => p.Running == true);
            var neigList = neigReader.GetAll();

            foreach (var ether in runningethers)
            {
                if (neigList.Count(i => i.Interface.Contains(ether.Name)) == 1)
                {
                    var neig = neigList.FirstOrDefault(n => n.Interface.Contains(ether.Name));
                    Logger.Information("En la interface {Interface} se encuentra un equipo {Modelo} con la MAC {MacAddress}", neig.Interface, neig.Board, neig.MacAddress);
                    if (neig.Address4 != "")
                    {
                        interfacesToTest.Add((neig.Interface, neig.Address4));
                        Logger.Information("Se agrego a la lista de interfaces a probar a la {Interface} con IP {Address}", neig.Interface, neig.Address4);
                    }
                    else
                    {
                        Logger.Error("El vecino en la {Interface} NO tiene IP y NO se agrega a la lista de pruebas", neig.Interface);
                    }
                }
                else
                {
                    Logger.Error("La interface {Interface} esta running y contiene NINGUN o VARIOS vecino/s", ether.Name);
                }
            }
            return interfacesToTest;
        }

        public List<(string, EthernetPoeStatus)> GetInterfacesPoeStatus(ITikConnection connection, IMonitoreable<MonitorPoeResults>[] poeReader)
        {
            var interfacesPoeStatus = new List<(string, EthernetPoeStatus)>();

            foreach (var ethPoe in poeReader)
            {
                var poeStatus = ethPoe.MonitorOnce(connection);
                Logger.Information("La interface {Interface} tiene un estado Poe {PoeStatus}", poeStatus.Name, poeStatus.PoeOutStatus);
                interfacesPoeStatus.Add((poeStatus.Name, poeStatus.PoeOutStatus));
            }

            return interfacesPoeStatus;
        }
    }
}
