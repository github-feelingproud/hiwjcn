/*
Navicat MySQL Data Transfer

Source Server         : 本地连接
Source Server Version : 50553
Source Host           : 127.0.0.1:3306
Source Database       : hiwjcn

Target Server Type    : MYSQL
Target Server Version : 50553
File Encoding         : 65001

Date: 2016-12-13 19:04:24
*/

SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for my_aspnet_sessions
-- ----------------------------
DROP TABLE IF EXISTS `my_aspnet_sessions`;
CREATE TABLE `my_aspnet_sessions` (
  `SessionId` varchar(191) NOT NULL,
  `ApplicationId` int(11) NOT NULL,
  `Created` datetime NOT NULL,
  `Expires` datetime NOT NULL,
  `LockDate` datetime NOT NULL,
  `LockId` int(11) NOT NULL,
  `Timeout` int(11) NOT NULL,
  `Locked` tinyint(1) NOT NULL,
  `SessionItems` longblob,
  `Flags` int(11) NOT NULL,
  PRIMARY KEY (`SessionId`,`ApplicationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for sundata
-- ----------------------------
DROP TABLE IF EXISTS `sundata`;
CREATE TABLE `sundata` (
  `IID` int(11) NOT NULL AUTO_INCREMENT,
  `UID` varchar(100) NOT NULL,
  `Data` varchar(3000) NOT NULL,
  `ImagePath` varchar(1000) NOT NULL,
  `Labels` varchar(1000) NOT NULL,
  PRIMARY KEY (`IID`)
) ENGINE=InnoDB AUTO_INCREMENT=94215 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for wp_address
-- ----------------------------
DROP TABLE IF EXISTS `wp_address`;
CREATE TABLE `wp_address` (
  `address_id` int(20) unsigned NOT NULL AUTO_INCREMENT,
  `user_id` int(20) NOT NULL,
  `ReceiverName` varchar(255) NOT NULL,
  `address` varchar(255) NOT NULL,
  `phone` varchar(255) DEFAULT NULL,
  `is_default` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`address_id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for wp_area
-- ----------------------------
DROP TABLE IF EXISTS `wp_area`;
CREATE TABLE `wp_area` (
  `primary_id` int(11) unsigned NOT NULL AUTO_INCREMENT,
  `area_id` varchar(50) NOT NULL,
  `parent_id` varchar(50) NOT NULL,
  `area_name` varchar(50) NOT NULL,
  `area_level` int(11) NOT NULL,
  `order_num` int(11) NOT NULL,
  PRIMARY KEY (`primary_id`)
) ENGINE=InnoDB AUTO_INCREMENT=47783 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for wp_category
-- ----------------------------
DROP TABLE IF EXISTS `wp_category`;
CREATE TABLE `wp_category` (
  `category_id` int(11) NOT NULL AUTO_INCREMENT,
  `category_name` varchar(255) NOT NULL,
  `category_description` varchar(255) DEFAULT NULL,
  `link_url` varchar(255) DEFAULT NULL,
  `category_image` varchar(255) DEFAULT NULL,
  `background_image` varchar(255) DEFAULT NULL,
  `category_color` varchar(255) DEFAULT NULL,
  `icon_class` varchar(255) DEFAULT NULL,
  `open_in_new_window` varchar(255) DEFAULT NULL,
  `order_num` int(11) DEFAULT '0',
  `category_parent` int(11) NOT NULL,
  `category_level` int(11) NOT NULL,
  `category_type` varchar(255) NOT NULL,
  PRIMARY KEY (`category_id`)
) ENGINE=InnoDB AUTO_INCREMENT=88 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for wp_comment
-- ----------------------------
DROP TABLE IF EXISTS `wp_comment`;
CREATE TABLE `wp_comment` (
  `comment_id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `comment_content` text NOT NULL,
  `user_id` int(11) NOT NULL,
  `thread_id` varchar(400) NOT NULL,
  `parent_comment_id` int(11) NOT NULL DEFAULT '-1',
  `update_time` datetime NOT NULL,
  PRIMARY KEY (`comment_id`)
) ENGINE=InnoDB AUTO_INCREMENT=32 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for wp_event
-- ----------------------------
DROP TABLE IF EXISTS `wp_event`;
CREATE TABLE `wp_event` (
  `EventID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `HotelNo` varchar(255) NOT NULL,
  `UserID` int(11) NOT NULL,
  `Content` varchar(255) NOT NULL,
  `SubContent` varchar(255) DEFAULT NULL,
  `TargetTitle` varchar(255) DEFAULT NULL,
  `TargetUrl` varchar(255) DEFAULT NULL,
  `CreatedTime` datetime NOT NULL,
  PRIMARY KEY (`EventID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for wp_hotel
-- ----------------------------
DROP TABLE IF EXISTS `wp_hotel`;
CREATE TABLE `wp_hotel` (
  `hotel_id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `hotel_no` varchar(100) NOT NULL,
  `hotel_name` varchar(100) NOT NULL,
  `hotel_desc` text,
  `province_id` varchar(255) DEFAULT NULL,
  `city_id` varchar(255) DEFAULT NULL,
  `area_id` varchar(255) DEFAULT NULL,
  `address` varchar(255) DEFAULT NULL,
  `lat` double DEFAULT NULL,
  `lng` double DEFAULT NULL,
  `create_time` datetime NOT NULL,
  `is_deleted` int(11) NOT NULL,
  PRIMARY KEY (`hotel_id`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for wp_hotel_manager
-- ----------------------------
DROP TABLE IF EXISTS `wp_hotel_manager`;
CREATE TABLE `wp_hotel_manager` (
  `map_id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `hotel_no` varchar(100) NOT NULL,
  `inviter_id` int(11) NOT NULL,
  `manager_id` int(11) NOT NULL,
  `MemberType` int(11) NOT NULL,
  `confirmed` int(11) NOT NULL,
  `create_time` datetime NOT NULL,
  `confirm_time` datetime DEFAULT NULL,
  PRIMARY KEY (`map_id`)
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for wp_links
-- ----------------------------
DROP TABLE IF EXISTS `wp_links`;
CREATE TABLE `wp_links` (
  `link_id` int(20) unsigned NOT NULL AUTO_INCREMENT,
  `link_url` varchar(255) NOT NULL DEFAULT '',
  `link_name` varchar(255) NOT NULL DEFAULT '',
  `link_target` varchar(25) NOT NULL DEFAULT '',
  `link_title` varchar(255) DEFAULT '',
  `link_image` varchar(255) DEFAULT '',
  `order_num` int(11) DEFAULT NULL,
  `link_owner` bigint(20) unsigned NOT NULL DEFAULT '1',
  `link_type` varchar(255) NOT NULL DEFAULT '',
  PRIMARY KEY (`link_id`)
) ENGINE=InnoDB AUTO_INCREMENT=68 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for wp_login_log
-- ----------------------------
DROP TABLE IF EXISTS `wp_login_log`;
CREATE TABLE `wp_login_log` (
  `log_id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `login_key` varchar(255) NOT NULL,
  `login_pwd` varchar(255) DEFAULT NULL,
  `login_ip` varchar(255) DEFAULT NULL,
  `login_time` datetime NOT NULL,
  PRIMARY KEY (`log_id`),
  KEY `login_key_index` (`login_key`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=14 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for wp_msg
-- ----------------------------
DROP TABLE IF EXISTS `wp_msg`;
CREATE TABLE `wp_msg` (
  `msg_id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `msg_title` varchar(255) NOT NULL DEFAULT '',
  `msg_content` text NOT NULL,
  `msg_time` datetime NOT NULL,
  `msg_sender` int(11) NOT NULL,
  `msg_receiver` int(11) NOT NULL,
  `msg_new` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`msg_id`)
) ENGINE=InnoDB AUTO_INCREMENT=19 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for wp_options
-- ----------------------------
DROP TABLE IF EXISTS `wp_options`;
CREATE TABLE `wp_options` (
  `option_id` bigint(20) unsigned NOT NULL AUTO_INCREMENT,
  `option_name` varchar(64) NOT NULL DEFAULT '',
  `option_value` longtext NOT NULL,
  PRIMARY KEY (`option_id`),
  UNIQUE KEY `option_name` (`option_name`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=833 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for wp_orders
-- ----------------------------
DROP TABLE IF EXISTS `wp_orders`;
CREATE TABLE `wp_orders` (
  `order_id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `order_num` varchar(255) NOT NULL,
  `customer_id` int(20) NOT NULL,
  `seller_id` int(20) NOT NULL,
  `total_money` decimal(10,0) unsigned NOT NULL,
  `offset_money` decimal(10,0) NOT NULL DEFAULT '0',
  `coupon_money` decimal(10,0) unsigned NOT NULL,
  `payed_money` decimal(10,0) NOT NULL,
  `create_time` datetime NOT NULL,
  `update_time` datetime NOT NULL,
  `pay_time` datetime NOT NULL,
  `reciever_name` varchar(255) DEFAULT NULL,
  `order_phone` varchar(255) DEFAULT NULL,
  `order_address` varchar(255) DEFAULT NULL,
  `pay_type` varchar(255) DEFAULT NULL,
  `third_trade_num` varchar(255) DEFAULT NULL,
  `pay_account` varchar(255) DEFAULT NULL,
  `target_account` varchar(255) DEFAULT NULL,
  `express_name` varchar(255) DEFAULT NULL,
  `express_number` varchar(255) DEFAULT NULL,
  `deliver_time` datetime NOT NULL,
  `finish_time` datetime NOT NULL,
  `order_remarks` varchar(255) DEFAULT NULL,
  `order_status` int(255) NOT NULL,
  `show_for_seller` varchar(255) NOT NULL DEFAULT '0',
  `show_for_customer` varchar(255) NOT NULL DEFAULT '0',
  PRIMARY KEY (`order_id`),
  UNIQUE KEY `unique_num` (`order_num`)
) ENGINE=InnoDB AUTO_INCREMENT=12 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for wp_orders_items
-- ----------------------------
DROP TABLE IF EXISTS `wp_orders_items`;
CREATE TABLE `wp_orders_items` (
  `order_item_id` bigint(20) unsigned NOT NULL AUTO_INCREMENT,
  `order_num` varchar(255) NOT NULL,
  `product_item_id` bigint(20) NOT NULL,
  `unit_price` decimal(10,0) NOT NULL,
  `product_count` int(10) unsigned NOT NULL,
  PRIMARY KEY (`order_item_id`)
) ENGINE=InnoDB AUTO_INCREMENT=15 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for wp_posts
-- ----------------------------
DROP TABLE IF EXISTS `wp_posts`;
CREATE TABLE `wp_posts` (
  `post_id` bigint(10) unsigned NOT NULL AUTO_INCREMENT,
  `HotelNo` varchar(255) NOT NULL DEFAULT '',
  `post_author` bigint(20) unsigned NOT NULL DEFAULT '0' COMMENT '作者',
  `post_date` datetime NOT NULL DEFAULT '0000-00-00 00:00:00' COMMENT '发布日期',
  `post_content` longtext NOT NULL,
  `post_title` varchar(300) NOT NULL,
  `post_preview` text,
  `post_guid` varchar(100) NOT NULL,
  `read_count` int(11) DEFAULT NULL,
  `sticky_top` varchar(255) DEFAULT NULL,
  `comment_count` int(20) DEFAULT '0',
  PRIMARY KEY (`post_id`)
) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for wp_price_temp
-- ----------------------------
DROP TABLE IF EXISTS `wp_price_temp`;
CREATE TABLE `wp_price_temp` (
  `temp_id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `room_type_id` int(11) NOT NULL,
  `temp_name` varchar(255) NOT NULL,
  `temp_desc` varchar(255) DEFAULT NULL,
  `temp_price` decimal(10,0) NOT NULL,
  `mon` int(11) NOT NULL DEFAULT '0',
  `tue` int(11) NOT NULL DEFAULT '0',
  `wed` int(11) NOT NULL DEFAULT '0',
  `thu` int(11) NOT NULL DEFAULT '0',
  `fri` int(11) NOT NULL DEFAULT '0',
  `sat` int(11) NOT NULL DEFAULT '0',
  `sun` int(11) NOT NULL DEFAULT '0',
  `start_date` datetime DEFAULT NULL,
  `end_date` datetime DEFAULT NULL,
  `temp_priority` int(11) NOT NULL DEFAULT '0',
  `is_active` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`temp_id`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for wp_products
-- ----------------------------
DROP TABLE IF EXISTS `wp_products`;
CREATE TABLE `wp_products` (
  `product_id` int(20) unsigned NOT NULL AUTO_INCREMENT,
  `product_title` varchar(255) NOT NULL,
  `max_price` decimal(10,0) DEFAULT NULL COMMENT 'item中最高价格',
  `min_price` decimal(10,0) DEFAULT NULL COMMENT 'item中最低价格',
  `product_count` int(11) DEFAULT NULL COMMENT 'item数量之和',
  `product_description` varchar(255) DEFAULT NULL,
  `product_content` longtext,
  `product_image` varchar(255) DEFAULT NULL,
  `product_tags` varchar(255) DEFAULT NULL,
  `brand_id` int(11) NOT NULL DEFAULT '0',
  `catalog_id` int(11) NOT NULL DEFAULT '0',
  `update_time` datetime NOT NULL,
  `product_link` varchar(255) DEFAULT NULL,
  `seller_id` int(20) NOT NULL,
  `show_on_home_page` varchar(255) DEFAULT NULL,
  `is_online` varchar(255) DEFAULT NULL,
  `is_deleted` varchar(255) NOT NULL DEFAULT '',
  PRIMARY KEY (`product_id`)
) ENGINE=InnoDB AUTO_INCREMENT=106 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for wp_products_items
-- ----------------------------
DROP TABLE IF EXISTS `wp_products_items`;
CREATE TABLE `wp_products_items` (
  `item_id` int(20) unsigned NOT NULL AUTO_INCREMENT,
  `item_title` varchar(255) NOT NULL,
  `product_item_price` decimal(10,0) NOT NULL,
  `product_item_count` int(11) NOT NULL,
  `max_single_buy_count` int(11) NOT NULL,
  `product_id` int(20) NOT NULL,
  `update_time` datetime NOT NULL,
  `is_deleted` varchar(255) NOT NULL,
  PRIMARY KEY (`item_id`)
) ENGINE=InnoDB AUTO_INCREMENT=15 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for wp_reqlog
-- ----------------------------
DROP TABLE IF EXISTS `wp_reqlog`;
CREATE TABLE `wp_reqlog` (
  `uid` bigint(20) unsigned NOT NULL AUTO_INCREMENT,
  `req_id` varchar(255) DEFAULT NULL,
  `req_ref_url` varchar(2000) DEFAULT NULL,
  `req_url` varchar(2000) DEFAULT NULL,
  `area_name` varchar(255) DEFAULT NULL,
  `controller_name` varchar(255) DEFAULT NULL,
  `action_name` varchar(255) DEFAULT NULL,
  `req_method` varchar(255) DEFAULT NULL,
  `post_params` longtext,
  `get_params` longtext,
  `req_time` double DEFAULT NULL,
  `update_time` datetime NOT NULL,
  PRIMARY KEY (`uid`)
) ENGINE=InnoDB AUTO_INCREMENT=729 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for wp_role
-- ----------------------------
DROP TABLE IF EXISTS `wp_role`;
CREATE TABLE `wp_role` (
  `role_id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `role_name` varchar(100) NOT NULL DEFAULT '',
  `role_desc` varchar(1000) DEFAULT '',
  `auto_assign_to_user` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`role_id`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for wp_role_permission
-- ----------------------------
DROP TABLE IF EXISTS `wp_role_permission`;
CREATE TABLE `wp_role_permission` (
  `role_permission_id` bigint(20) NOT NULL AUTO_INCREMENT,
  `role_id` bigint(20) NOT NULL,
  `permission_id` bigint(20) NOT NULL,
  PRIMARY KEY (`role_permission_id`)
) ENGINE=InnoDB AUTO_INCREMENT=18 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for wp_room
-- ----------------------------
DROP TABLE IF EXISTS `wp_room`;
CREATE TABLE `wp_room` (
  `room_id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `hotel_no` varchar(255) NOT NULL,
  `room_no` varchar(255) NOT NULL,
  `RoomDesc` varchar(1000) DEFAULT NULL,
  `RoomImagesJson` varchar(10000) DEFAULT NULL,
  `room_type` int(11) NOT NULL,
  `room_status` int(11) NOT NULL,
  `order_no` varchar(255) DEFAULT NULL,
  `male_count` int(11) DEFAULT NULL,
  `female_count` int(11) DEFAULT NULL,
  `order_start` datetime DEFAULT NULL,
  `order_end` datetime DEFAULT NULL,
  `sort_no` int(11) NOT NULL DEFAULT '0',
  `CreatedTime` datetime NOT NULL,
  PRIMARY KEY (`room_id`)
) ENGINE=InnoDB AUTO_INCREMENT=31 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for wp_room_type
-- ----------------------------
DROP TABLE IF EXISTS `wp_room_type`;
CREATE TABLE `wp_room_type` (
  `type_id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `type_name` varchar(255) NOT NULL,
  `type_desc` varchar(255) DEFAULT NULL,
  `type_price` decimal(10,0) unsigned NOT NULL,
  `hotel_no` varchar(255) NOT NULL,
  `created_time` datetime NOT NULL,
  `update_time` datetime DEFAULT NULL,
  PRIMARY KEY (`type_id`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for wp_section
-- ----------------------------
DROP TABLE IF EXISTS `wp_section`;
CREATE TABLE `wp_section` (
  `section_id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `section_name` varchar(255) NOT NULL,
  `section_title` varchar(255) DEFAULT NULL,
  `section_description` varchar(255) DEFAULT NULL,
  `section_content` text,
  `section_type` varchar(255) DEFAULT NULL,
  `rel_group` varchar(255) DEFAULT NULL,
  `update_time` datetime DEFAULT NULL,
  PRIMARY KEY (`section_id`),
  UNIQUE KEY `unique_name` (`section_name`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=23 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for wp_shopping_cart
-- ----------------------------
DROP TABLE IF EXISTS `wp_shopping_cart`;
CREATE TABLE `wp_shopping_cart` (
  `cart_id` int(20) unsigned NOT NULL AUTO_INCREMENT,
  `product_item_id` int(20) NOT NULL,
  `product_item_count` int(11) NOT NULL,
  `customer_id` int(20) NOT NULL,
  `update_time` datetime NOT NULL,
  PRIMARY KEY (`cart_id`)
) ENGINE=InnoDB AUTO_INCREMENT=12 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for wp_tag
-- ----------------------------
DROP TABLE IF EXISTS `wp_tag`;
CREATE TABLE `wp_tag` (
  `tag_id` int(11) NOT NULL AUTO_INCREMENT,
  `tag_name` varchar(100) NOT NULL,
  `tag_desc` varchar(255) DEFAULT '',
  `tag_link` varchar(255) DEFAULT '',
  `item_count` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`tag_id`),
  UNIQUE KEY `name_unique` (`tag_name`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=37 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for wp_tag_map
-- ----------------------------
DROP TABLE IF EXISTS `wp_tag_map`;
CREATE TABLE `wp_tag_map` (
  `map_id` int(11) NOT NULL AUTO_INCREMENT,
  `map_key` varchar(255) NOT NULL,
  `tag_name` varchar(255) NOT NULL,
  `map_type` varchar(255) NOT NULL,
  PRIMARY KEY (`map_id`)
) ENGINE=InnoDB AUTO_INCREMENT=37 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for wp_upfile
-- ----------------------------
DROP TABLE IF EXISTS `wp_upfile`;
CREATE TABLE `wp_upfile` (
  `UpID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `UserID` int(11) NOT NULL,
  `FileName` varchar(255) NOT NULL,
  `FileSize` int(11) NOT NULL,
  `FileExt` varchar(255) NOT NULL,
  `FilePath` varchar(255) DEFAULT NULL,
  `FileUrl` varchar(255) NOT NULL,
  `FileMD5` varchar(255) NOT NULL,
  `UpTime` datetime NOT NULL,
  PRIMARY KEY (`UpID`)
) ENGINE=InnoDB AUTO_INCREMENT=314 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for wp_users
-- ----------------------------
DROP TABLE IF EXISTS `wp_users`;
CREATE TABLE `wp_users` (
  `user_id` int(32) unsigned NOT NULL AUTO_INCREMENT,
  `UID` varchar(40) NOT NULL DEFAULT '',
  `nick_name` varchar(250) NOT NULL DEFAULT '',
  `user_email` varchar(200) NOT NULL DEFAULT '',
  `user_pass` varchar(64) NOT NULL DEFAULT '',
  `user_phone` varchar(20) DEFAULT NULL,
  `user_money` decimal(10,2) unsigned zerofill NOT NULL DEFAULT '00000000.00',
  `user_sex` varchar(10) DEFAULT '',
  `user_qq` varchar(50) DEFAULT NULL,
  `user_mark` text,
  `user_img` varchar(100) DEFAULT '',
  `user_db_img` mediumblob,
  `user_reg_time` datetime NOT NULL DEFAULT '0000-00-00 00:00:00',
  `user_flag` int(50) NOT NULL DEFAULT '-1',
  PRIMARY KEY (`user_id`),
  UNIQUE KEY `邮箱唯一` (`user_email`) USING BTREE,
  KEY `密码索引` (`user_pass`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=42 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for wp_user_role
-- ----------------------------
DROP TABLE IF EXISTS `wp_user_role`;
CREATE TABLE `wp_user_role` (
  `user_role_id` bigint(20) unsigned NOT NULL AUTO_INCREMENT,
  `user_id` bigint(20) NOT NULL,
  `role_id` bigint(20) NOT NULL,
  PRIMARY KEY (`user_role_id`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8;

-- ----------------------------
-- View structure for v_hotelmanager
-- ----------------------------
DROP VIEW IF EXISTS `v_hotelmanager`;
CREATE ALGORITHM=UNDEFINED DEFINER=`root`@`localhost` SQL SECURITY DEFINER VIEW `v_hotelmanager` AS select `wp_hotel_manager`.`map_id` AS `map_id`,`wp_hotel_manager`.`hotel_no` AS `hotel_no`,`wp_hotel_manager`.`inviter_id` AS `inviter_id`,`wp_hotel_manager`.`manager_id` AS `manager_id`,`wp_hotel_manager`.`MemberType` AS `MemberType`,`wp_hotel_manager`.`confirmed` AS `confirmed`,`wp_hotel_manager`.`create_time` AS `create_time`,`wp_hotel_manager`.`confirm_time` AS `confirm_time` from `wp_hotel_manager` ;
