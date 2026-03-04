using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherSystem : MonoBehaviour
{
    [Header("Start Date")]
    [Min(1)] public int startYear = 1;
    [Range(1, 12)] public int startMonth = 1;
    [Range(1, 10)] public int startDay = 1;

    [Header("Time Settings")]
    public float dayLengthInSeconds = 120f;
    public int daysPerMonth = 10;
    public int monthsPerYear = 12;

    [Header("Weather Objects")]
    public GameObject rain;
    public GameObject heavyRain;
    public GameObject thunderstorm;
    public GameObject snow;
    public GameObject heavySnow;
    public GameObject cloud;

    [Header("Sky Objects")]
    public GameObject skyDefault;
    public GameObject skyCloud;
    public GameObject skyRain;
    public GameObject skyHeavyRain;
    public GameObject skyThunder;
    public GameObject skySnow;
    public GameObject skyHeavySnow;

    [Header("Debug Controls")]
    public KeyCode nextDayKey = KeyCode.L;

    private int currentDay = 1;
    private int currentMonth = 1;
    private int currentYear = 1;

    private Season currentSeason;
    private WeatherType currentWeather = WeatherType.Default;

    private float dayTimer;

    void Start()
    {
        InitializeDate();
        UpdateSeason();
        StartNewDay();
    }

    void Update()
    {
        dayTimer += Time.deltaTime;

        if (dayTimer >= dayLengthInSeconds)
        {
            NextDay();
        }
        if (Input.GetKeyDown(nextDayKey))
        {
            NextDay();
        }

    }
    void InitializeDate()
    {
        currentYear = Mathf.Max(1, startYear);

        currentMonth = Mathf.Clamp(startMonth, 1, monthsPerYear);
        currentDay = Mathf.Clamp(startDay, 1, daysPerMonth);

        dayTimer = 0f;
    }
    void NextDay()
    {
        dayTimer = 0f;

        currentDay++;

        if (currentDay > daysPerMonth)
        {
            currentDay = 1;
            currentMonth++;

            if (currentMonth > monthsPerYear)
            {
                currentMonth = 1;
                currentYear++;
            }

            UpdateSeason();
        }

        StartNewDay();
    }

    void UpdateSeason()
    {
        if (currentMonth == 12 || currentMonth <= 2)
            currentSeason = Season.Winter;
        else if (currentMonth <= 5)
            currentSeason = Season.Spring;
        else if (currentMonth <= 8)
            currentSeason = Season.Summer;
        else
            currentSeason = Season.Autumn;
    }

    void StartNewDay()
    {
        RollWeather();
        ApplyWeather();
    }

    void RollWeather()
    {
        int roll = UnityEngine.Random.Range(0, 100);

        switch (currentSeason)
        {
            case Season.Summer:
                currentWeather = GetWeatherFromRoll(roll, 45, 15, 20, 10, 10, 0, 0);
                break;

            case Season.Autumn:
                currentWeather = GetWeatherFromRoll(roll, 5, 30, 25, 20, 5, 15, 0);
                break;

            case Season.Winter:
                currentWeather = GetWeatherFromRoll(roll, 30, 20, 0, 0, 0, 35, 15);
                break;

            case Season.Spring:
                currentWeather = GetWeatherFromRoll(roll, 40, 15, 25, 10, 10, 0, 0);
                break;
        }
    }

    WeatherType GetWeatherFromRoll(int roll,
        int defaultChance,
        int cloudChance,
        int rainChance,
        int heavyRainChance,
        int thunderChance,
        int snowChance,
        int heavySnowChance)
    {
        int cumulative = 0;

        cumulative += defaultChance;
        if (roll < cumulative) return WeatherType.Default;

        cumulative += cloudChance;
        if (roll < cumulative) return WeatherType.Cloud;

        cumulative += rainChance;
        if (roll < cumulative) return WeatherType.Rain;

        cumulative += heavyRainChance;
        if (roll < cumulative) return WeatherType.HeavyRain;

        cumulative += thunderChance;
        if (roll < cumulative) return WeatherType.Thunderstorm;

        cumulative += snowChance;
        if (roll < cumulative) return WeatherType.Snow;

        cumulative += heavySnowChance;
        if (roll < cumulative) return WeatherType.HeavySnow;

        return WeatherType.Default;
    }

    void ApplyWeather()
    {
        DisableAllWeather();

        switch (currentWeather)
        {
            case WeatherType.Default:
                skyDefault.SetActive(true);
                break;

            case WeatherType.Cloud:
                cloud.SetActive(true);
                skyCloud.SetActive(true);
                break;

            case WeatherType.Rain:
                rain.SetActive(true);
                skyRain.SetActive(true);
                break;

            case WeatherType.HeavyRain:
                heavyRain.SetActive(true);
                skyHeavyRain.SetActive(true);
                break;

            case WeatherType.Thunderstorm:
                thunderstorm.SetActive(true);
                skyThunder.SetActive(true);
                break;

            case WeatherType.Snow:
                snow.SetActive(true);
                skySnow.SetActive(true);
                break;

            case WeatherType.HeavySnow:
                heavySnow.SetActive(true);
                skyHeavySnow.SetActive(true);
                break;
        }

        Debug.Log($"Year {currentYear}, Month {currentMonth}, Day {currentDay} | Season: {currentSeason} | Weather: {currentWeather}");
    }

    void DisableAllWeather()
    {
        cloud.SetActive(false);
        rain.SetActive(false);
        heavyRain.SetActive(false);
        thunderstorm.SetActive(false);
        snow.SetActive(false);
        heavySnow.SetActive(false);

        skyDefault.SetActive(false);
        skyCloud.SetActive(false);
        skyRain.SetActive(false);
        skyHeavyRain.SetActive(false);
        skyThunder.SetActive(false);
        skySnow.SetActive(false);
        skyHeavySnow.SetActive(false);
    }
}

public enum Season
{
    Winter,
    Spring,
    Summer,
    Autumn
}

public enum WeatherType
{
    Default,
    Cloud,
    Rain,
    HeavyRain,
    Thunderstorm,
    Snow,
    HeavySnow
}