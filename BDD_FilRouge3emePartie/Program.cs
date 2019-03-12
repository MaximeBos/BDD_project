using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using MySql.Data.MySqlClient;
using System.IO;
using Newtonsoft.Json;

namespace BDD_FilRouge3emePartie
{//Projet de BOS Maxime et LENEUF Antoine-Felix
    class Program
    {

        static string[] Affichage_M1()
        {
            string nomDuDocXML = "M1.xml";

            //créer l'arborescence des chemins XPath du document
            //--------------------------------------------------
            XPathDocument doc = new System.Xml.XPath.XPathDocument(nomDuDocXML);
            XPathNavigator nav = doc.CreateNavigator();

            //créer une requete XPath
            //-----------------------
            string maRequeteXPath = "/Reservation/*";
            XPathExpression expr = nav.Compile(maRequeteXPath);

            //exécution de la requete
            //-----------------------
            XPathNodeIterator nodes = nav.Select(expr);// exécution de la requête XPath

            //parcourir le resultat
            //---------------------
            string[] nodeContent = { "", "", "", "" };

            while (nodes.MoveNext()) // pour chaque réponses XPath (on est au niveau d'un noeud BD)
            {

                nodeContent[0] = nodes.Current.ToString(); //nom du client
                nodes.MoveNext();
                nodeContent[1] = nodes.Current.ToString(); //adresse client
                nodes.MoveNext();
                nodeContent[2] = nodes.Current.ToString(); //date
                nodes.MoveNext();
                nodeContent[3] = nodes.Current.ToString();  //arrondissement = theme (ici 16e)
            }
            return nodeContent;
        }

        static string[] E2(string[] nodeContent)
        {
            string nom_client = nodeContent[0];
            string adresse_client = nodeContent[1];
            string date_client = nodeContent[2];
            string arrondissement_client = "16e"; //cela nous servira pour les programmes suivant...
            string ID_client = "pas encore ID";
            string[] info_client = { nom_client, adresse_client, date_client, arrondissement_client, ID_client }; //pour plus de lisibilite

            //string connectionString = "SERVER=localhost; PORT =3306;DATABASE=BOS_MAXI;UID=root;PASSWORD=Maximedu33360;";
            string connectionString = "SERVER=fboisson.ddns.net; PORT =3306;DATABASE=BOS_MAXI;UID=S6-BOS-MAXI;PASSWORD=12066;";
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();
            MySqlCommand command = connection.CreateCommand();
            command.CommandText =
                    " SELECT* FROM client ;";

            MySqlDataReader reader;
            reader = command.ExecuteReader();

            string nom = " ";
            bool est_present = false;
            while (reader.Read() && est_present != true)            
            {
                nom = reader.GetString("nom_client");  
                if (nom == nom_client)
                {
                    string ID = reader.GetString("ID_client");
                    info_client[4] = ID;
                    est_present = true;
                    Console.WriteLine("Le client est deja present dans la base de donnée.\n");
                }
            }

            if (nom != nom_client)
            {
                Console.WriteLine("Le client n'est pas deja enregistre dans la base de donne.\nAjout du client dans la base de donnee...\n");
                int ID_nouveau_client = 021;
                info_client[4] = ID_nouveau_client.ToString();
                connection.Close();
                connection.Open();
                MySqlCommand command2 = connection.CreateCommand();
                command2.CommandText =
                       "INSERT INTO `BOS_MAXI`.`client` (`ID_client`, `nom_client`, `tel`, `email`, `adresse`) VALUES(@ID_client, @nom_client, 0635201978, 'morray@devinci.fr', @adresse);";
                command2.Parameters.AddWithValue("@nom_client", nom_client);
                command2.Parameters.AddWithValue("@ID_client", ID_nouveau_client);
                command2.Parameters.AddWithValue("@adresse", adresse_client);
                MySqlDataReader reader2;
                reader2 = command2.ExecuteReader();
                connection.Close();
                Console.WriteLine("Le client " + nom_client + "  vient d'etre ajoute a la base de donnee.\n");
            }

            return info_client;
        }

        static string[] E3(string [] client)
        {

            string[] info_client = { client[0], client[1], client[2], client[3], client[4], "", "", "" };
            //string[] info_client = { nom_client, adresse_client, date_client, arrondissement_client, ID_client, immat_client, nom_parking, num_place };
            
            //string connectionString = "SERVER=localhost; PORT =3306;DATABASE=BOS_MAXI;UID=root;PASSWORD=Maximedu33360;";
            string connectionString = "SERVER=fboisson.ddns.net; PORT =3306;DATABASE=BOS_MAXI;UID=S6-BOS-MAXI;PASSWORD=12066;";
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();
            MySqlCommand command = connection.CreateCommand();
            command.CommandText =
                    " SELECT* FROM voiture;";
            MySqlDataReader reader;
            reader = command.ExecuteReader();
            reader.Read();
            string arrondissement = reader.GetString("arrondissement");
            string dispo = reader.GetString("disponibilite");

            string disponibilite = " ";
            bool existe = false;
            bool voiture_dispo = false;
            while (reader.Read() && existe != true)                           
            {

                if (arrondissement == info_client[3] && dispo == "disponible") //si il y a une voiture disponible dans le parking de l'arrondissement desire par le client (ici 16e)
                {
                    info_client[5] = reader.GetString("immat");
                    info_client[7] = reader.GetString("num_place");
                    existe = true;
                    Console.WriteLine("La voiture d immatriculation " + info_client[5] + " est disponible dans le parking du "+ info_client[3] + " a la place " + info_client[7] + ".\n");

                    MySqlConnection connection3 = new MySqlConnection(connectionString);
                    connection3.Open();
                    MySqlCommand command1 = connection3.CreateCommand();
                    command1.CommandText =
                           "SELECT nom_parking FROM parking WHERE arrondissement=@info_client"; //va chercher le nom du parking de l'arrondissement choisi par le client
                    command1.Parameters.AddWithValue("@info_client", info_client[3]);   
                    MySqlDataReader reader1;
                    reader1 = command1.ExecuteReader();
                    reader1.Read();
                    info_client[6] = reader1.GetString("nom_parking");
                    connection3.Close();
                }
            }
            connection.Close();
            
            if (arrondissement != info_client[3] && voiture_dispo != true) //si il n'y a pas de voiture disponible dans le parking de l'arrondissement desire par le client (ici 16e)
            {
                connection.Open();
                MySqlCommand command2 = connection.CreateCommand();
                command2.CommandText =
                       "SELECT* FROM voiture";
                MySqlDataReader reader2;
                reader2 = command2.ExecuteReader();
                while (reader2.Read() && voiture_dispo != true)
                {
                    disponibilite = reader2.GetString("disponibilite");
                    if (disponibilite == "disponible")
                    {
                        Console.WriteLine("Aucune voiture n'est disponible dans le parking du " + info_client[3] + " arrondissement.\n");
                        Console.WriteLine("Recherche d'une voiture disponible...");
                        voiture_dispo = true;
                        string arrondissement_initial = "";
                        arrondissement_initial = reader2.GetString("arrondissement"); //recupere l'arrondissement du parking ou la voiture est disponible
                        info_client[5] = reader2.GetString("immat"); //recupere l'immatriculation de la voiture disponible a deplacer dans le parking de l'arrondissement demande par le client (ici 16e)
                        Console.WriteLine("La voiture d immatriculation " + info_client[5] + " est disponible.\n");


                        info_client[7] = "A10"; //nouvelle place dans le parking de l'arrondissement demande par le client
                        MySqlConnection connection2 = new MySqlConnection(connectionString);
                        connection2.Open();
                        MySqlCommand command3 = connection2.CreateCommand();
                        command3.CommandText =
                              "UPDATE voiture SET arrondissement=@arrond, num_place=@num_place WHERE immat=@immat;"; //actualise la voiture deplacee dans le parking de l'arrondissement choisi par le client (ici 16e)
                        command3.Parameters.AddWithValue("@immat", info_client[5]);
                        command3.Parameters.AddWithValue("@num_place", info_client[7]);
                        command3.Parameters.AddWithValue("@arrond", info_client[3]);
                        MySqlDataReader reader3;
                        reader3 = command3.ExecuteReader();
                        connection2.Close();
                        Console.WriteLine("Deplacement de la voiture dans le parking du " + info_client[3] + " arrondissement...\n");


                        connection2.Open();
                        MySqlCommand command7 = connection2.CreateCommand();
                        command7.CommandText =
                            "SELECT nb_places FROM parking WHERE arrondissement=@arrondissement;"; //recupere le nombre de place du parking qui s'est vu retire une voiture
                        command7.Parameters.AddWithValue("@arrondissement", info_client[3]);
                        MySqlDataReader reader7;
                        reader7 = command7.ExecuteReader();
                        reader7.Read();
                        int nb_places = reader7.GetInt32("nb_places") - 1;
                        connection2.Close();


                        connection2.Open();
                        MySqlCommand command4 = connection2.CreateCommand();
                        command4.CommandText =
                               "UPDATE parking SET nb_places=@nb_places WHERE arrondissement=@arrondissement;"; //actualise le nombre de place du parking du 16e
                        command4.Parameters.AddWithValue("@arrondissement", info_client[3]);
                        command4.Parameters.AddWithValue("@nb_places", nb_places);
                        MySqlDataReader reader4;
                        reader4 = command4.ExecuteReader();
                        connection2.Close();
                        

                        connection2.Open();
                        MySqlCommand command5 = connection2.CreateCommand();
                        command5.CommandText =
                            "SELECT nb_places FROM parking WHERE arrondissement=@arrondissement_initial;"; //recupere le nombre de place du parking qui s'est vu retire une voiture
                        command5.Parameters.AddWithValue("@arrondissement_initial", arrondissement_initial);
                        MySqlDataReader reader5;
                        reader5 = command5.ExecuteReader();
                        reader5.Read();
                        int nb_places2 = reader5.GetInt32("nb_places") + 1;
                        connection2.Close();

                        connection2.Open();
                        MySqlCommand command6 = connection2.CreateCommand();
                        command6.CommandText =
                               "UPDATE parking SET nb_places=@nb_places WHERE arrondissement=@arrondissement;"; //actualise le nombre de place du parking qui s'est vu retire la voiture
                        command6.Parameters.AddWithValue("@arrondissement", arrondissement_initial);
                        command6.Parameters.AddWithValue("@nb_places", nb_places2);
                        MySqlDataReader reader6;
                        reader6 = command6.ExecuteReader();
                        connection2.Close();

                        connection2.Open();
                        MySqlCommand command8 = connection2.CreateCommand();
                        command8.CommandText =
                               "SELECT nom_parking FROM parking WHERE arrondissement=@arrond;"; //recupere le nom du parking correspondant a l'arrondissement choisi du client
                        command8.Parameters.AddWithValue("@arrond", info_client[3]);
                        MySqlDataReader reader8;
                        reader8 = command8.ExecuteReader();
                        reader8.Read();
                        info_client[6] = reader8.GetString("nom_parking");
                        connection2.Close();
                        Console.WriteLine("La voiture d'immatriculation " + info_client[5] + " a ete deplace dans le " + info_client[6] + " du " + info_client[3] + " arrondissement.\n");
                    }
                }
                connection.Close();
            }
            return info_client;
        }

        static List<Logement> DeserialiserFileJsonToObject()
        {

            //ouverture d'un stream en lecture vers le fichier Réponse de RBNP pour la semaine 14 de lannée 2018.json
            StreamReader fileReader = new StreamReader("Réponse de RBNP pour la semaine 14 de lannée 2018.json");
            JsonSerializer jsonSerialiser = new JsonSerializer();


            //Désérialisation à partir du flux de lecture
            List<Logement> ListeLogements = (List<Logement>)jsonSerialiser.Deserialize(fileReader, typeof(List<Logement>));
            //fermeture des reader ouvert
            fileReader.Close();
            return ListeLogements;
        }


        static Logement E5 (List<Logement> ListeLogements) //selectionne l'appartement retenu en fonction des criteres du client
        {
            Logement l = null;
            foreach (Logement log in ListeLogements)
            {
                if ((log.Borough == "16" && log.Bedrooms == 1 && log.Overall_satisfaction >= 4.5  && log.Availability == "yes"))
                {
                            { l = log; break; }
                }
            }
            return l; 
        }

        
        static void SerialiserObjectToFileJson(Logement l)
        {
            //instanciations des outils
            //StreamWriter et JsonTextWriter
            //
            StreamWriter fileWriter = new StreamWriter("reponse RBNP.json");
            JsonTextWriter jsonWriter = new JsonTextWriter(fileWriter);
            JsonSerializer serializer = new JsonSerializer();
            //
            serializer.Serialize(jsonWriter, l);
            //-----------

            //Sérialisation
            //
            String json = JsonConvert.SerializeObject(l);
            //

            //fermeture des writer
            //
            fileWriter.Close();
            jsonWriter.Close();

            //relecture du fichier créé
            //-----------------------------
            Console.WriteLine("Lecture du fichier 'reponse RBNP.json' comportant l'appartement selectionne...\n");
        }

        static void AfficherPrettyJson(string nomFichier)
        {
            StreamReader reader = new StreamReader(nomFichier);
            JsonTextReader jreader = new JsonTextReader(reader);
            while (jreader.Read())
            {
                if (jreader.Value != null)
                {
                    if (jreader.TokenType.ToString() == "PropertyName")
                    {
                        Console.Write(jreader.Value + " : ");
                    }
                    else
                    {
                        Console.WriteLine(jreader.Value);
                    }
                }
                else
                {
                    // Console.WriteLine("Token:{0} ", jreader.TokenType.ToString());
                    if (jreader.TokenType.ToString() == "StartObject") Console.WriteLine("Appartement\n--------------");
                    if (jreader.TokenType.ToString() == "EndObject") Console.WriteLine("-------------\n");
                    if (jreader.TokenType.ToString() == "StartArray") Console.WriteLine("Liste\n");
                }
            }
            jreader.Close();
            reader.Close();
        }

        static void Reservation_Sejour(string[] info_client, Logement l)
        {
            //string connectionString = "SERVER=localhost; PORT =3306;DATABASE=BOS_MAXI;UID=root;PASSWORD=Maximedu33360;";
            string connectionString = "SERVER=fboisson.ddns.net; PORT =3306;DATABASE=BOS_MAXI;UID=S6-BOS-MAXI;PASSWORD=12066;";
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();
         

            MySqlCommand command = connection.CreateCommand();
            command.CommandText =
                    "INSERT INTO `BOS_MAXI`.`sejour` (`num_sejour`, `ID_client`, `theme`, `date`, `validation_sejour`, `immat`, `prix_sejour`, `num_reservation`, `note`, `commentaire`) VALUES ( 'A3', @ID_client, @theme, @date, 'sejour non valide', @immat, @prix_sejour, @num_reservation, null, ' ');";
            command.Parameters.AddWithValue("@ID_client", Convert.ToInt32(info_client[4]));
            command.Parameters.AddWithValue("@theme", info_client[3]);
            command.Parameters.AddWithValue("@date", info_client[2]);
            command.Parameters.AddWithValue("@immat", info_client[5]);
            command.Parameters.AddWithValue("@prix_sejour", l.Price);
            command.Parameters.AddWithValue("@num_reservation", l.Room_id);
            MySqlDataReader reader;
            reader = command.ExecuteReader();
            connection.Close();
            Console.WriteLine("Enregistrement du sejour en mode non valide...\n");
            Console.WriteLine("num_sejour : A3\nID_client : " + info_client[4] + "\n" + "theme : " + info_client[3] + "\ndate : " + info_client[2] + "\nvalidation_sejour : sejour non valide\nimmat " + info_client[5] + "\nprix du sejour " + l.Price + "\nnum_reservation " + l.Room_id + "\n");

        }
        
        static void E6_Emission_M2(Logement l, string[] info_client)
        {

            XmlDocument docXml = new XmlDocument();

            // création de l'en-tête XML (no <=> pas de DTD associée)
            docXml.CreateXmlDeclaration("1.0", "UTF-8", "no");

            XmlElement racine = docXml.CreateElement("Sejour");
            docXml.AppendChild(racine);

            XmlElement Theme = docXml.CreateElement("Theme");
            Theme.InnerText = info_client[3];
            racine.AppendChild(Theme);

            XmlElement Date = docXml.CreateElement("Date");
            Date.InnerText = info_client[2];
            racine.AppendChild(Date);

            XmlElement Info = docXml.CreateElement("Information");
            Info.InnerText = "sejour valide";
            racine.AppendChild(Info);

            XmlElement Client = docXml.CreateElement("Client");
            racine.AppendChild(Client);

            XmlElement Nom = docXml.CreateElement("Nom");
            Nom.InnerText = info_client[0];
            Client.AppendChild(Nom);

            XmlElement Numero_Client = docXml.CreateElement("Num_Client");
            Numero_Client.InnerText = info_client[4];
            Client.AppendChild(Numero_Client);

            XmlElement Location_voiture = docXml.CreateElement("Location_Voiture");
            racine.AppendChild(Location_voiture);

            XmlElement Nom_Parking = docXml.CreateElement("Nom_Parking");
            Nom_Parking.InnerText = info_client[6];
            Location_voiture.AppendChild(Nom_Parking);

            XmlElement Num_Place = docXml.CreateElement("Numero_Place");
            Num_Place.InnerText = info_client[7];
            Location_voiture.AppendChild(Num_Place);

            XmlElement Immat = docXml.CreateElement("Immatriculation");
            Immat.InnerText = info_client[5];
            Location_voiture.AppendChild(Immat);

            XmlElement Location_appartement = docXml.CreateElement("Location_Appartement");
            racine.AppendChild(Location_appartement);

            XmlElement Numero_Appart = docXml.CreateElement("Numero_Appartement");
            Numero_Appart.InnerText = l.Host_id.ToString();
            Location_appartement.AppendChild(Numero_Appart);

            XmlElement Identif_chambre = docXml.CreateElement("Identifiant_Chambre");
            Identif_chambre.InnerText = l.Room_type;
            Location_appartement.AppendChild(Identif_chambre);

            // enregistrement du document XML   ==> à retrouver dans le dossier bin\Debug de Visual Studio
            docXml.Save("M2.xml");
        }

        static string[] Affichage_M2()
        {
            string nomDuDocXML = "M2.xml";

            //créer l'arborescence des chemins XPath du document
            //--------------------------------------------------
            XPathDocument doc = new System.Xml.XPath.XPathDocument(nomDuDocXML);
            XPathNavigator nav = doc.CreateNavigator();

            //créer une requete XPath
            //-----------------------
            string maRequeteXPath = "/Sejour/*";
            XPathExpression expr = nav.Compile(maRequeteXPath);

            //exécution de la requete
            //-----------------------
            XPathNodeIterator nodes = nav.Select(expr);// exécution de la requête XPath

            //parcourir le resultat
            //---------------------
            string[] nodeContent = { "", "", "", "", "", "", "", "", "", ""};
            Console.WriteLine("Affichage du message de reservation envoye au client...\n");
            while (nodes.MoveNext()) 
            {

                nodeContent[0] = nodes.Current.ToString();
                nodes.Current.MoveToNext();
                nodeContent[1] = nodes.Current.ToString();
                nodes.Current.MoveToNext();
                nodeContent[2] = nodes.Current.ToString();
                nodes.Current.MoveToNext();
                nodes.Current.MoveToFirstChild();
                nodeContent[3] = nodes.Current.ToString();
                nodes.Current.MoveToNext();
                nodeContent[4] = nodes.Current.ToString();
                nodes.Current.MoveToParent();
                nodes.Current.MoveToNext();
                nodes.Current.MoveToFirstChild();
                nodeContent[5] = nodes.Current.ToString();
                nodes.Current.MoveToNext();
                nodeContent[6] = nodes.Current.ToString();
                nodes.Current.MoveToNext();
                nodeContent[7] = nodes.Current.ToString();
                nodes.Current.MoveToParent();
                nodes.Current.MoveToNext();
                nodes.Current.MoveToFirstChild();
                nodeContent[8] = nodes.Current.ToString();
                nodes.Current.MoveToNext();
                nodeContent[9] = nodes.Current.ToString();
                break;

            }
            return nodeContent;
        }

        static void Analyse_M3(string[] info_client, Logement l)
        {
            string nomDuDocXML = "M3.xml";

            //créer l'arborescence des chemins XPath du document
            //--------------------------------------------------
            XPathDocument doc = new System.Xml.XPath.XPathDocument(nomDuDocXML);
            XPathNavigator nav = doc.CreateNavigator();

            //créer une requete XPath
            //-----------------------
            string maRequeteXPath = "/Validation_sejour/*";
            XPathExpression expr = nav.Compile(maRequeteXPath);

            //exécution de la requete
            //-----------------------
            XPathNodeIterator nodes = nav.Select(expr);// exécution de la requête XPath

            //parcourir le resultat
            //---------------------
            string[] nodeContent = { "", ""};
            while (nodes.MoveNext())
            {
                nodeContent[0] = nodes.Current.ToString(); //ID sejour
                nodes.MoveNext();
                nodeContent[1] = nodes.Current.ToString(); //information sejour valide
                break;
            }
            Console.WriteLine("Affichage du message de validation du client...\n");
            Console.WriteLine("ID sejour : " + nodeContent[0] + "\nInformation : " + nodeContent[1] + "\n\n");

            if (nodeContent[1] == "sejour valide")
            {
                //string connectionString = "SERVER=localhost; PORT =3306;DATABASE=BOS_MAXI;UID=root;PASSWORD=Maximedu33360;";
                string connectionString = "SERVER=fboisson.ddns.net; PORT =3306;DATABASE=BOS_MAXI;UID=S6-BOS-MAXI;PASSWORD=12066;";
                MySqlConnection connection = new MySqlConnection(connectionString);
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText =
                       "UPDATE sejour SET validation_sejour=@info WHERE num_sejour=@num;"; //actualise le sejour a sejour valide
                command.Parameters.AddWithValue("@info", nodeContent[1]);
                command.Parameters.AddWithValue("@num", nodeContent[0]);
                MySqlDataReader reader;
                reader = command.ExecuteReader();
                connection.Close();

                connection.Open();
                MySqlCommand command1 = connection.CreateCommand();
                command1.CommandText =
                       "SELECT immat FROM sejour WHERE num_sejour=@num;"; //selectionne immatriculation
                command1.Parameters.AddWithValue("@num", nodeContent[0]);
                
                MySqlDataReader reader1;
                reader1 = command1.ExecuteReader();
                reader1.Read();
                string immat = "";
                immat = reader1.GetString("immat");
                connection.Close();
    

                connection.Open();
                MySqlCommand command2 = connection.CreateCommand();
                command2.CommandText =
                       "UPDATE voiture SET disponibilite='en location' WHERE immat=@immatriculation;"; //actualise le statut du vehicule
                command2.Parameters.AddWithValue("@immatriculation", immat);
                MySqlDataReader reader2;
                reader2 = command2.ExecuteReader();
                connection.Close();

                Console.WriteLine("Enregistrement du sejour...\n");
                Console.WriteLine("num_sejour : A3\nID_client : " + info_client[4] + "\n" + "theme : " + info_client[3] + "\ndate : " + info_client[2] + "\nvalidation_sejour : sejour valide\nimmat " + info_client[5] + "\nprix du sejour " + l.Price + "\nnum_reservation " + l.Room_id + "\n");

            }

            else
            {
                Console.WriteLine("Annulation du sejour...");
            }
        }

        static void CheckOut (string[] info_client, Logement l)
        {
            //string connectionString = "SERVER=localhost; PORT =3306;DATABASE=BOS_MAXI;UID=root;PASSWORD=Maximedu33360;";
            string connectionString = "SERVER=fboisson.ddns.net; PORT =3306;DATABASE=BOS_MAXI;UID=S6-BOS-MAXI;PASSWORD=12066;";

            //Le client laisse la voiture a la place ou il l'a trouve
            MySqlConnection connection = new MySqlConnection(connectionString);

            Console.WriteLine("Fin du sejour, le client emet une note et un avis :\nAffichage du sejour :\nnum_sejour : A3\nID_client : " + info_client[4] + "\n" + "theme : " + info_client[3] + "\ndate : " + info_client[2] + "\nvalidation_sejour : sejour valide\nimmat " + info_client[5] + "\nprix du sejour " + l.Price + "\nnum_reservation " + l.Room_id + "\nnote : 5\ncommentaire ; Sejour tres agreable. Recommande vivement !\n");
            //choix arbitraire de la note et du commentaire 

            connection.Open();
            MySqlCommand command3 = connection.CreateCommand();
            command3.CommandText =
                    "SELECT nom_controleur FROM voiture WHERE immat=@immat;"; //la voiture part en maintenance donc est attribuee au parking 0
            command3.Parameters.AddWithValue("@immat", info_client[5]);
            MySqlDataReader reader3;
            reader3 = command3.ExecuteReader();
            reader3.Read();
            string nom_controleur = reader3.GetString("nom_controleur");
            connection.Close();
            Console.WriteLine("Le client rends la voiture qui va etre controlee par le controleur " + nom_controleur + ".\n");

            connection.Open();
            MySqlCommand command = connection.CreateCommand();
            command.CommandText =
                    "UPDATE voiture SET arrondissement='0', num_place='0', disponibilite='en maintenance' WHERE immat=@immat;"; //la voiture part en maintenance donc est attribuee au parking 0
            command.Parameters.AddWithValue("@immat", info_client[5]);
            MySqlDataReader reader;
            reader = command.ExecuteReader();
            connection.Close();

            Console.WriteLine("Apres controle, il s'avere que la voiture a besoin d'un nettoyage, description de la maintenance :\n");

            connection.Open();
            MySqlCommand command1 = connection.CreateCommand();
            command1.CommandText =
                    "INSERT INTO `BOS_MAXI`.`maintenance` (`codeM`, `type`, `date`, `immat`) VALUES('1', 'nettoyage', 'semaine 15', @immat2);";  //maintenance de la voiture, nettoyage
            command1.Parameters.AddWithValue("@immat2", info_client[5]);
            MySqlDataReader reader1;
            reader1 = command1.ExecuteReader();
            Console.WriteLine("codeM : 1\ntype : nettoyage\ndate: semaine 15\nimmatriculation : " + info_client[5] + ".\n");
            connection.Close();

            Console.WriteLine("Le nettoyage de la voiture a bien ete effectue. La voiture est de nouveau disponible dans le parking du " + info_client[3] + " arrondissement a la place " + info_client[7] + ".\n");
            connection.Open();
            MySqlCommand command2 = connection.CreateCommand();
            command2.CommandText =
                    "UPDATE voiture SET disponibilite='disponible', num_place=@num_place, arrondissement=@arrond WHERE immat=@immat2;"; //la voiture est deplacee dans le parking ou le client l'a laisse
            command2.Parameters.AddWithValue("@immat2", info_client[5]);
            command2.Parameters.AddWithValue("@arrond", info_client[3]);
            command2.Parameters.AddWithValue("@num_place", info_client[7]);
            MySqlDataReader reader2;
            reader2 = command2.ExecuteReader();
            connection.Close();
        }
        //Enregistrement de la position de la voiture rendue par le client 
        //string[] info_client = { nom_client, adresse_client, date_client, arrondissement_client, ID_client, immat_client, nom_parking, num_place };

        static void Historique_Client(string[] info_client)
        {
            //string connectionString = "SERVER=localhost; PORT =3306;DATABASE=BOS_MAXI;UID=root;PASSWORD=Maximedu33360;";
            string connectionString = "SERVER=fboisson.ddns.net; PORT =3306;DATABASE=BOS_MAXI;UID=S6-BOS-MAXI;PASSWORD=12066;";
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();
            MySqlCommand command = connection.CreateCommand();
            command.CommandText =
                    " SELECT* FROM client WHERE nom_client=@client;";
            command.Parameters.AddWithValue("@client", info_client[0]);
            MySqlDataReader reader;
            reader = command.ExecuteReader();
            while (reader.Read())     // parcours ligne par ligne
            {
                reader.Read();
                string ID = reader.GetString("ID_client");
                string nom = reader.GetString("nom_client");
                string tel = reader.GetString("tel");
                string email = reader.GetString("email");
                string adresse = reader.GetString("adresse");
                Console.WriteLine("Affichage de l'historique du client " + info_client[0] + " dans la base de donnee :\nID_client : " + ID + "\nnom_client : " + nom + "\ntel : " + tel + "\nemail : " + email + "\adresse : " + adresse + ".\n");
            }
            connection.Close();
        } //affiche l'historique du client dans la base de donnee

        static void Historique_Maintenance(string[] info_client)
        {
            //string connectionString = "SERVER=localhost; PORT =3306;DATABASE=BOS_MAXI;UID=root;PASSWORD=Maximedu33360;";
            string connectionString = "SERVER=fboisson.ddns.net; PORT =3306;DATABASE=BOS_MAXI;UID=S6-BOS-MAXI;PASSWORD=12066;";
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();
            MySqlCommand command = connection.CreateCommand();
            command.CommandText =
                    " SELECT* FROM maintenance WHERE immat=@immat;";
            command.Parameters.AddWithValue("@immat", info_client[5]);
            MySqlDataReader reader;
            reader = command.ExecuteReader();
            Console.WriteLine("Affichage de l'historique de maintenance de la voiture immatriculee " + info_client[5] + " de la base de donnee :\n");
            while (reader.Read())    
            {
                string codeM = reader.GetString("codeM");
                string type = reader.GetString("type");
                string date = reader.GetString("date");
                Console.WriteLine("codeM : " + codeM + "\ntype : " + type + "\ndate : " + date + ".\n");
            }
            connection.Close();
        } //affiche l'historique des maintenance de la voiture dans la base de donnee

        static void Rentabilite_Voiture(string[] info_client)
        {
            //string connectionString = "SERVER=localhost; PORT =3306;DATABASE=BOS_MAXI;UID=root;PASSWORD=Maximedu33360;";
            string connectionString = "SERVER=fboisson.ddns.net; PORT =3306;DATABASE=BOS_MAXI;UID=S6-BOS-MAXI;PASSWORD=12066;";
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();
            MySqlCommand command = connection.CreateCommand();
            command.CommandText =
                    "SELECT COUNT(*) FROM sejour WHERE immat=@immat;";
            command.Parameters.AddWithValue("@immat", info_client[5]);
            MySqlDataReader reader;
            reader = command.ExecuteReader();
            reader.Read();
            int nb_location = reader.GetInt32(0);
            Console.WriteLine("La voiture immatriculee " + info_client[5] + " a ete loue " + nb_location + " fois.\n");
            connection.Close();


            connection.Open();
            MySqlCommand command1 = connection.CreateCommand();
            command1.CommandText =
                    "SELECT COUNT(*) FROM maintenance WHERE immat=@immat;";
            command1.Parameters.AddWithValue("@immat", info_client[5]);
            MySqlDataReader reader1;
            reader1 = command1.ExecuteReader();
            reader1.Read();
            int nb_maintenance = reader1.GetInt32(0);
            connection.Close();
            Console.WriteLine("La voiture immatriculee " + info_client[5] + " a ete " + nb_maintenance + " fois en maintenance.\n");

            if (nb_location > nb_maintenance)
            {
                Console.WriteLine("La voiture a ete louee plus de fois qu'elle n'a ete en maintenance, on peut donc considerer qu'elle est rentable.\n");
            }

            if (nb_location < nb_maintenance)
            {
                Console.WriteLine("La voiture a ete en maintenance plus de fois qu'elle n'a ete louee, on peut donc considerer qu'elle n'est pas rentable.\n");
            }
            else
            {
                Console.WriteLine("La voiture a ete loue autant de fois qu'elle a ete en maintenance, on peut donc considerer qu'elle n'est pas rentable.\n");
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Affichage du message de reservation de sejour...\n");
            string[] nodeContent = Affichage_M1();
            Console.WriteLine("nom client: " + nodeContent[0] + "\nAdresse: " + nodeContent[1] + "\nDate: " + nodeContent[2] + "\nSéjour: " + nodeContent[3] + "\n");
            Console.WriteLine("\nAppuyez sur une touche pour passer à la suite...\n");
            Console.ReadKey();


            Console.WriteLine("Recherche du client dans la base de donnee...\n");
            string[] info_client = E2(nodeContent);
            Console.WriteLine("Verification d'une voiture disponible dans le parking du " + info_client[3] + " arrondissement...\n");
            string[] info = E3(info_client);
            Console.WriteLine("\nAppuyez sur une touche pour passer à la suite...\n");
            Console.ReadKey();


            DeserialiserFileJsonToObject();
            Console.WriteLine("Recherche d'un appartement disponible dans le 16ème arrondissement avce 1 chambre d'evaluation minimum de 4.5...\n");
            Logement loge = E5(DeserialiserFileJsonToObject());
            SerialiserObjectToFileJson(loge);
            Console.WriteLine("Appartement trouve.\nAffichage de l'appartement selectionne..\n");
            AfficherPrettyJson("reponse RBNP.json");
            Console.WriteLine("\n");


            Reservation_Sejour(info, loge);


            E6_Emission_M2(loge, info);
            string[] nodeContent2 = { "", "", "", "", "", "", "", "", "", "" };
            nodeContent2 = Affichage_M2();
            Console.WriteLine("theme: " + nodeContent2[0] + "\nDate: " + nodeContent2[1] + "\nInformation: " + nodeContent2[2] + "\nClient :\n   Nom :" + nodeContent2[3] + "\n" + 
                "   Num_Client :" + nodeContent2[4] + "\nLocation Voiture :\n    Nom parking : " + nodeContent2[5] + "\n    Numero place : " + nodeContent2[6] + "\n" +
                "    Immatriculation : " + nodeContent2[7] + "\nAppartement :\n    Numero Appartement : " + nodeContent2[8] + "\n    Identifiant chambre : " + nodeContent2[9] + "\n");
            Console.WriteLine("\nAppuyez sur une touche pour passer à la suite...\n");
            Console.ReadKey();


            Console.WriteLine("En attente du message de confirmation du client...\n");
            Analyse_M3(info, loge);
            Console.WriteLine("\nAppuyez sur une touche pour passer a la suite...\n");
            Console.ReadKey();

            CheckOut(info, loge);
            Console.WriteLine("\nAppuyez sur une touche pour passer a la suite...\n");
            Console.ReadKey();

            Historique_Client(info);
            Console.WriteLine("\nAppuyez sur une touche pour passer a la suite...\n");
            Console.ReadKey();

            Historique_Maintenance(info);
            Console.WriteLine("\nAppuyez sur une touche pour passer a la suite...\n");
            Console.ReadKey();

            Console.WriteLine("Affichage de la rentabilite du vehicule...\n");
            Rentabilite_Voiture(info);
            Console.WriteLine("\nDemo terminee, appuyez sur une touche pour quitter.");
            Console.ReadKey();
        }
    }
}

