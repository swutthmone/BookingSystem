-- MySQL dump 10.13  Distrib 8.0.23, for Win64 (x86_64)
--
-- Host: localhost    Database: bookingsystem
-- ------------------------------------------------------
-- Server version	8.0.23

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `tbl_booking_transaction`
--

DROP TABLE IF EXISTS `tbl_booking_transaction`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `tbl_booking_transaction` (
  `BookingTransactionID` int NOT NULL AUTO_INCREMENT,
  `UserID` int NOT NULL,
  `ClassID` int NOT NULL,
  `Type` int NOT NULL DEFAULT '0' COMMENT '0= Booking, 1 = Waiting',
  `Credit` int NOT NULL DEFAULT '0',
  `Status` int NOT NULL DEFAULT '0' COMMENT '0=Pending,1=Cancel,2= Complete',
  `BookingDate` datetime DEFAULT CURRENT_TIMESTAMP,
  `CreatedDate` datetime DEFAULT CURRENT_TIMESTAMP,
  `ModifiedDate` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`BookingTransactionID`),
  KEY `ClassID` (`ClassID`),
  KEY `UserID_1` (`UserID`),
  CONSTRAINT `ClassID` FOREIGN KEY (`ClassID`) REFERENCES `tbl_class` (`ClassID`),
  CONSTRAINT `UserID_1` FOREIGN KEY (`UserID`) REFERENCES `tbl_user` (`UserID`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tbl_booking_transaction`
--

LOCK TABLES `tbl_booking_transaction` WRITE;
/*!40000 ALTER TABLE `tbl_booking_transaction` DISABLE KEYS */;
INSERT INTO `tbl_booking_transaction` VALUES (1,2,1,0,1,2,'2023-11-27 20:15:24','2023-11-27 20:15:24','2023-11-27 20:15:25'),(2,3,1,0,1,1,'2023-11-27 20:36:40','2023-11-27 20:36:40','2023-11-27 21:20:28'),(4,4,1,0,1,0,'2023-11-27 23:45:02','2023-11-27 23:38:36','2023-11-27 23:51:18');
/*!40000 ALTER TABLE `tbl_booking_transaction` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tbl_class`
--

DROP TABLE IF EXISTS `tbl_class`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `tbl_class` (
  `ClassID` int NOT NULL AUTO_INCREMENT,
  `PackageID` int NOT NULL,
  `ClassName` varchar(200) DEFAULT NULL,
  `Credit` int NOT NULL,
  `MaxBookingCount` int NOT NULL,
  `StartTime` time DEFAULT NULL,
  `StartDate` date DEFAULT NULL,
  `EndDate` date DEFAULT NULL,
  `CreatedDate` datetime DEFAULT CURRENT_TIMESTAMP,
  `ModifiedDate` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`ClassID`),
  KEY `PackageID_idx` (`PackageID`),
  CONSTRAINT `PackageID` FOREIGN KEY (`PackageID`) REFERENCES `tbl_packages` (`PackageID`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tbl_class`
--

LOCK TABLES `tbl_class` WRITE;
/*!40000 ALTER TABLE `tbl_class` DISABLE KEYS */;
INSERT INTO `tbl_class` VALUES (1,1,'1 Hr Yoga(SG)',1,2,'09:00:00','2023-11-27','2023-12-31','2023-11-27 12:28:00','2023-11-27 12:28:00'),(2,1,'2 Hr Yoga(SG)',2,10,'12:00:00','2023-11-28','2023-11-30','2023-11-27 12:28:42','2023-11-27 12:28:42'),(3,3,'1 Hr Yoga(Myanmar)',5,15,'13:00:00','2023-11-23','2023-11-30','2023-11-27 12:29:37','2023-11-27 12:29:37');
/*!40000 ALTER TABLE `tbl_class` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tbl_country`
--

DROP TABLE IF EXISTS `tbl_country`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `tbl_country` (
  `CountryID` int NOT NULL AUTO_INCREMENT,
  `CountryName` varchar(200) DEFAULT NULL,
  `CountryCode` varchar(200) DEFAULT NULL,
  `CreatedDate` datetime DEFAULT CURRENT_TIMESTAMP,
  `ModifiedDate` datetime DEFAULT NULL,
  PRIMARY KEY (`CountryID`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tbl_country`
--

LOCK TABLES `tbl_country` WRITE;
/*!40000 ALTER TABLE `tbl_country` DISABLE KEYS */;
INSERT INTO `tbl_country` VALUES (1,'Australia','61','2023-11-27 00:00:00','2023-11-27 10:55:37'),(2,'Belgium','32','2023-11-27 10:55:48','2023-11-27 10:55:48'),(3,'Cambodia','855','2023-11-27 10:56:24','2023-11-27 10:56:24'),(4,'Myanmar','95','2023-11-27 10:56:31','2023-11-27 10:56:57'),(5,'Singapore','65','2023-11-27 10:56:53','2023-11-27 10:56:53');
/*!40000 ALTER TABLE `tbl_country` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tbl_packages`
--

DROP TABLE IF EXISTS `tbl_packages`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `tbl_packages` (
  `PackageID` int NOT NULL AUTO_INCREMENT,
  `CountryID` int NOT NULL,
  `PackageName` varchar(200) DEFAULT NULL,
  `Credit` int NOT NULL,
  `Price` decimal(18,2) NOT NULL DEFAULT '0.00',
  `ExpiredDate` date NOT NULL,
  `CreatedDate` datetime DEFAULT CURRENT_TIMESTAMP,
  `ModifiedDate` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`PackageID`),
  KEY `CountryID_idx` (`CountryID`),
  CONSTRAINT `CountryID` FOREIGN KEY (`CountryID`) REFERENCES `tbl_country` (`CountryID`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tbl_packages`
--

LOCK TABLES `tbl_packages` WRITE;
/*!40000 ALTER TABLE `tbl_packages` DISABLE KEYS */;
INSERT INTO `tbl_packages` VALUES (1,5,'Basic Package SG',5,50.00,'2023-12-27','2023-11-27 11:23:05','2023-11-27 11:23:05'),(2,5,'Special Package SG',10,120.00,'2023-11-27','2023-11-27 11:26:08','2023-11-27 11:26:08'),(3,4,'Basic Package Myanmar',20,200.00,'2023-12-31','2023-11-27 11:26:35','2023-11-27 11:26:35');
/*!40000 ALTER TABLE `tbl_packages` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tbl_tmp_user`
--

DROP TABLE IF EXISTS `tbl_tmp_user`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `tbl_tmp_user` (
  `UserID` int NOT NULL AUTO_INCREMENT,
  `UserName` varchar(200) DEFAULT NULL,
  `FirstName` varchar(200) DEFAULT NULL,
  `LastName` varchar(200) DEFAULT NULL,
  `Gender` tinyint(1) NOT NULL DEFAULT '0' COMMENT '0=male, 1=female',
  `DOB` date DEFAULT NULL,
  `Email` varchar(200) DEFAULT NULL,
  `OTP` varchar(200) DEFAULT NULL,
  `OTPExpireTime` datetime DEFAULT NULL,
  `OTPFailCount` int NOT NULL DEFAULT '0',
  `IsVerified` tinyint(1) DEFAULT '0',
  `CreatedDate` datetime DEFAULT CURRENT_TIMESTAMP,
  `ModifiedDate` datetime DEFAULT NULL,
  PRIMARY KEY (`UserID`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tbl_tmp_user`
--

LOCK TABLES `tbl_tmp_user` WRITE;
/*!40000 ALTER TABLE `tbl_tmp_user` DISABLE KEYS */;
INSERT INTO `tbl_tmp_user` VALUES (1,'Test User1','test','user 1',1,'1992-11-27','testuser@gmail.com','C-549239','2023-11-27 14:24:15',0,1,'2023-11-26 22:03:23','2023-11-26 22:09:50'),(2,'test user 2','test','user 2',0,'1993-11-27','testuser2@gmail.com','M-318876','2023-11-27 14:24:15',1,1,'2023-11-27 14:15:45','2023-11-27 14:19:15'),(5,'test user 3','test','user 3',0,'1990-11-27','testuser3@gmail.com','P-801624','2023-11-27 23:05:25',0,1,'2023-11-27 23:00:25','2023-11-27 23:01:18');
/*!40000 ALTER TABLE `tbl_tmp_user` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tbl_user`
--

DROP TABLE IF EXISTS `tbl_user`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `tbl_user` (
  `UserID` int NOT NULL AUTO_INCREMENT,
  `UserName` varchar(200) DEFAULT NULL,
  `FirstName` varchar(200) DEFAULT NULL,
  `LastName` varchar(200) DEFAULT NULL,
  `Gender` tinyint(1) NOT NULL DEFAULT '0' COMMENT '0=male,1=female',
  `DOB` date DEFAULT NULL,
  `Email` varchar(200) DEFAULT NULL,
  `OTP` varchar(200) DEFAULT NULL,
  `OTPExpireTime` datetime DEFAULT NULL,
  `OTPFailCount` int NOT NULL DEFAULT '0',
  `Password` varchar(50) DEFAULT NULL,
  `Salt` varchar(200) DEFAULT NULL,
  `CreatedDate` datetime DEFAULT CURRENT_TIMESTAMP,
  `ModifiedDate` datetime DEFAULT NULL,
  `AccessStatus` int DEFAULT '0',
  `LoginFailCount` int DEFAULT '0',
  PRIMARY KEY (`UserID`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tbl_user`
--

LOCK TABLES `tbl_user` WRITE;
/*!40000 ALTER TABLE `tbl_user` DISABLE KEYS */;
INSERT INTO `tbl_user` VALUES (2,'Test User1','test','user 1',1,'1992-11-27','testuser@gmail.com','L-526060','2023-11-27 18:42:01',0,'by5BfLssUSaL1vFl5wHzgNTe2Dey74uJ','k68yQ/bI3nFYu7X4a08SXfIabHXE6WQ2','2023-11-27 18:37:01','2023-11-27 18:37:47',0,0),(3,'test user 2','test','user 2',0,'1993-11-27','testuser2@gmail.com','C-685705','2023-11-27 20:21:58',0,'5xK3aCMv28vyVQAMEk6agVgZVr+jyAA0','szvGmhcduxwhpBiZmWwgIWhxuIEs0xtn','2023-11-27 20:16:58','2023-11-27 20:17:53',0,0),(4,'test user 3','test','user 3',0,'1990-11-27','testuser3@gmail.com','S-203829','2023-11-27 23:09:21',0,'7GvwoUm1bGmm/CTOy8XCxaQWMUcjHfLB','Snlq1WUGycXve/HVWenziAzCLA5vRg9D','2023-11-27 23:02:36','2023-11-27 23:04:41',0,0);
/*!40000 ALTER TABLE `tbl_user` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tbl_user_transaction`
--

DROP TABLE IF EXISTS `tbl_user_transaction`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `tbl_user_transaction` (
  `TransactionID` int NOT NULL AUTO_INCREMENT,
  `UserID` int NOT NULL,
  `PackageID` int NOT NULL,
  `Type` int NOT NULL DEFAULT '0' COMMENT '0= BuyPacakge, 1 = BookingOrWaiting',
  `Credit` int NOT NULL DEFAULT '0',
  `Debit` int NOT NULL DEFAULT '0',
  `CreatedDate` datetime DEFAULT CURRENT_TIMESTAMP,
  `ModifiedDate` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`TransactionID`),
  KEY `UserID_idx` (`UserID`),
  KEY `PackageID_idx` (`PackageID`),
  CONSTRAINT `UserID` FOREIGN KEY (`UserID`) REFERENCES `tbl_user` (`UserID`)
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tbl_user_transaction`
--

LOCK TABLES `tbl_user_transaction` WRITE;
/*!40000 ALTER TABLE `tbl_user_transaction` DISABLE KEYS */;
INSERT INTO `tbl_user_transaction` VALUES (1,2,1,0,5,0,'2023-11-27 16:20:08','2023-11-27 16:20:08'),(3,2,2,0,10,0,'2023-11-27 17:26:23','2023-11-27 17:26:23'),(4,2,1,1,0,1,'2023-11-27 20:15:31','2023-11-27 20:15:31'),(5,3,1,0,5,0,'2023-11-27 20:36:06','2023-11-27 20:36:06'),(6,3,1,1,0,1,'2023-11-27 20:36:40','2023-11-27 20:36:40'),(7,4,1,0,5,0,'2023-11-27 23:35:16','2023-11-27 23:35:16'),(8,4,1,1,0,0,'2023-11-27 23:36:44','2023-11-27 23:51:15');
/*!40000 ALTER TABLE `tbl_user_transaction` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2023-11-28  0:01:41
