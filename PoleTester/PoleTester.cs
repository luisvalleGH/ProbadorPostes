﻿using Eternet.Mikrotik;
using Eternet.Mikrotik.Entities.Interface;
using Eternet.Mikrotik.Entities.Interface.Ethernet;
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
        private readonly ILogger _logger;
        private readonly ITikConnection _connection;

        public PoleTester(ILogger logger, ITikConnection connection)
        {
            _logger = logger;
            _connection = connection;
        }

        public List<(string iface, string ip)> GetNeighborsOnRunningInterfaces(
            IEntityReader<InterfaceEthernet> ethReader, IEntityReader<IpNeighbor> neigReader)
        {
            var results = new List<(string iface, string ip)>();
            var runningethers = ethReader.GetAll().Where(p => p.Running);
            var neigList = neigReader.GetAll();

            foreach (var ether in runningethers)
            {
                var lista = neigList.Where(n => n.Interface.ToLower().Equals(ether.Name.ToLower())).ToArray();
                var neig = lista.FirstOrDefault();
                if (neig != null && lista.Length == 1)
                {
                    _logger.Information(
                        "En la interface {Interface} se encuentra un equipo {Modelo} con la MAC {MacAddress}",
                        neig.Interface, neig.Board, neig.MacAddress);
                    if (!string.IsNullOrEmpty(neig.Address4))
                    {
                        results.Add((neig.Interface, neig.Address4));
                        _logger.Information(
                            "Se agrego a la lista de interfaces a probar a la {Interface} con IP {Address}",
                            neig.Interface, neig.Address4);
                    }
                    else
                    {
                        _logger.Error("El vecino en la {Interface} NO tiene IP y NO se agrega a la lista de pruebas",
                            neig.Interface);
                    }
                }
                else
                {
                    _logger.Error("La interface {Interface} esta running y contiene NINGUN o VARIOS vecino/s",
                        ether.Name);
                }
            }

            return results;
        }

        public List<(string iface, EthernetPoeStatus poeStatus)> GetInterfacesPoeStatus(
            IMonitoreable<MonitorPoeResults>[] poeReader)
        {
            var results = new List<(string, EthernetPoeStatus)>();
            foreach (var ethPoe in poeReader)
            {
                var poeStatus = ethPoe.MonitorOnce(_connection);
                _logger.Information("La interface {Interface} tiene un estado Poe {PoeStatus}", poeStatus.Name,
                    poeStatus.PoeOutStatus);
                results.Add((poeStatus.Name, poeStatus.PoeOutStatus));
            }

            return results;
        }

        public List<(string name, string autonegotiation, bool fullduplex, EthernetRates rate)> GetInterfacesNegotiation(
            IEntityReader<InterfaceEthernet> ethReader)
        {
            var interfacesRunningNegotiation = new List<(string name, string autonegotiation, bool fullduplex, EthernetRates rate)>();

            var allIfaces = ethReader.GetAll();
            var allRunning = allIfaces.Where(i => i.Running);

            foreach (var iface in allRunning)
            {
                var negoStatus = iface.MonitorOnce(_connection);
                _logger.Information(
                    "Interface {Interface}, Autonegotiation {AutoStatus}, Full Dulplex {FullStatus}, Rate {RateStatus}",
                    negoStatus.Name, negoStatus.AutoNegotiation, negoStatus.FullDuplex, negoStatus.Rate);
                interfacesRunningNegotiation.Add((negoStatus.Name, negoStatus.AutoNegotiation, negoStatus.FullDuplex,
                    negoStatus.Rate));
            }

            return interfacesRunningNegotiation;
        }

        public List<(string duration, long txAverage, long rxAverage, long lostPackets)> RunBandwithTests
            (List<(string iface, string ip)> ifacesToTest, List<(string name, string autonegotiation, bool fullduplex, EthernetRates rate)> ifacesNegotiation)
        {
            var btList = new List<(string duration, long txAverage, long rxAverage, long lostPackets)>();

            foreach (var iface in ifacesToTest)
            {
                var name = iface.iface;
                var nego = ifacesNegotiation.Find(n => n.name == name).rate.ToString();
                //case
            }

            return btList;
        }
    }
}
