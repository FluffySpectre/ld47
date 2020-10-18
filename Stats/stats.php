<?php

$statsFilename = 'stats.json';
$jsonReturnKey = 'kefZGS33zfjke56248Ljhdt5673874SFKseL';

if (isset($_GET['key']) && $_GET['key'] === $jsonReturnKey && file_exists($statsFilename)) {
    header('Content-type:application/json;charset=utf-8');
    die(file_get_contents($statsFilename));
}

$platform = isset($_POST['platform']) ? $_POST['platform'] : null;
$sessionId = isset($_POST['sessionId']) ? $_POST['sessionId'] : null;

$allowedPlatforms = ['WebGLPlayer', 'WindowsPlayer', 'OSXPlayer'];

$ignoreStatsFromIp = '95.90.226.36';
if ($_SERVER['REMOTE_ADDR'] === $ignoreStatsFromIp) die('Ignore stats from me');

if ($platform !== null && $sessionId !== null && in_array($platform, $allowedPlatforms)) {
    // read in the json file
    $json = null;
    if (file_exists($statsFilename)) {
        $string = file_get_contents($statsFilename);
        $json = json_decode($string, true); 
    } else {
        $json = ['times_played' => 0, 'web' => 0, 'windows' => 0, 'mac' => 0, 'received_stats' => []];
    }

    // add the new data to the existing one
    $json['times_played']++;
    if ($platform === 'WebGLPlayer') $json['web']++;
    else if ($platform === 'WindowsPlayer') $json['windows']++;
    else if ($platform === 'OSXPlayer') $json['mac']++;

    $json['received_stats'][] = ['timestamp' => date('Y-m-d H:i:s'), 'sessionId' => $sessionId, 'platform' => $platform];

    // save the json file
    $newJsonString = json_encode($json, JSON_PRETTY_PRINT);
    file_put_contents($statsFilename, $newJsonString);
}
