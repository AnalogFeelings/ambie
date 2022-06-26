﻿using System;
using System.Collections.Generic;
using System.Text;
using AmbientSounds.Models;

namespace AmbientSounds.Services
{
    public sealed class FocusHistoryService : IFocusHistoryService
    {
        private FocusHistory? _activeHistory;

        public event EventHandler<FocusHistory?>? HistoryAdded;

        public void TrackHistoryCompletion(long utcTicks, SessionType lastCompletedSegmentType)
        {
            if (_activeHistory is null)
            {
                return;
            }

            TrackSegmentEnd(lastCompletedSegmentType);
            _activeHistory.EndUtcTicks = utcTicks;

            // TODO save to cache

            HistoryAdded?.Invoke(this, _activeHistory);
            _activeHistory = null;
        }

        public void TrackIncompleteHistory(
            long utcTicks,
            SessionType partialSegmentType,
            TimeSpan minutesUsedInIncompleteSegment)
        {
            if (_activeHistory is null)
            {
                return;
            }

            _activeHistory.PartialSegmentType = partialSegmentType;
            _activeHistory.PartialSegmentTicks = minutesUsedInIncompleteSegment.Ticks;
            _activeHistory.EndUtcTicks = utcTicks;

            // TODO save to cache

            HistoryAdded?.Invoke(this, _activeHistory);
            _activeHistory = null;
        }

        public void TrackNewHistory(long utcTicks, int focusLength, int restLength, int repetitions)
        {
            _activeHistory = new FocusHistory
            {
                StartUtcTicks = utcTicks,
                FocusLength = focusLength,
                RestLength = restLength,
                Repetitions = repetitions
            };
        }

        public void TrackSegmentEnd(SessionType sessionType)
        {
            if (_activeHistory is null)
            {
                return;
            }

            if (sessionType is SessionType.Focus)
            {
                _activeHistory.FocusSegmentsCompleted++;
            }
            else if (sessionType is SessionType.Rest)
            {
                _activeHistory.RestSegmentsCompleted++;
            }
        }
    }
}