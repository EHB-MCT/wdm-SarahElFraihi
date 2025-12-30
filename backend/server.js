const express = require("express");
const mongoose = require("mongoose");
const cors = require("cors");

const app = express();
const port = 3000;

app.use(express.json());
app.use(cors());

const dbUrl = process.env.DB_URL || "mongodb://mongo:27017/gamedata";
mongoose.connect(dbUrl).then(() => console.log("✅ MongoDB Connected"));

const ActionSchema = new mongoose.Schema({
	uid: String,
	questionId: String,
	choiceWeight: Number,
	traitTarget: String,
	responseTimeMs: Number,
	mouseDistance: Number,
	timestamp: { type: Date, default: Date.now },
});

const Action = mongoose.model("Action", ActionSchema);
function calculateOceanProfile(uid, actions) {
	let profile = {
		openness: 50,
		conscientiousness: 50,
		extraversion: 50,
		agreeableness: 50,
		neuroticism: 50,
	};

	if (actions.length > 0) {
		actions.forEach((a) => {
			if (a.traitTarget === "Agreeableness")
				profile.agreeableness += a.choiceWeight * 10;
			if (a.traitTarget === "Openness") profile.openness += a.choiceWeight * 10;
		});

		const avgDistance =
			actions.reduce((sum, a) => sum + (a.mouseDistance || 0), 0) /
			actions.length;
		if (avgDistance > 1000) profile.neuroticism += 20;
		if (avgDistance < 200) profile.neuroticism -= 10;

		const avgTime =
			actions.reduce((sum, a) => sum + (a.responseTimeMs || 0), 0) /
			actions.length;
		if (avgTime < 1500) profile.conscientiousness -= 20;
		if (avgTime > 4000 && avgTime < 10000) profile.conscientiousness += 15;
		if (avgTime < 2000) profile.extraversion += 15;
	}

	Object.keys(profile).forEach(
		(k) => (profile[k] = Math.min(100, Math.max(0, profile[k])))
	);

	let verdict = "HIRE";
	if (profile.neuroticism > 70 || profile.conscientiousness < 30)
		verdict = "REJECT";

	return { uid, ...profile, verdict };
}

app.post("/api/track", async (req, res) => {
	try {
		const newAction = new Action(req.body);
		await newAction.save();
		res.json({ status: "success" });
	} catch (error) {
		res.status(500).json({ error: error.message });
	}
});

app.get("/api/profile/:uid", async (req, res) => {
	try {
		const actions = await Action.find({ uid: req.params.uid });
		const profile = calculateOceanProfile(req.params.uid, actions);
		res.json(profile);
	} catch (error) {
		res.status(500).json({ error: error.message });
	}
});

app.get("/api/dashboard", async (req, res) => {
	try {
		const allActions = await Action.find({});

		const usersData = {};
		allActions.forEach((a) => {
			if (!usersData[a.uid]) usersData[a.uid] = [];
			usersData[a.uid].push(a);
		});

		const summaries = Object.keys(usersData).map((uid) => {
			return calculateOceanProfile(uid, usersData[uid]);
		});

		res.json(summaries);
	} catch (error) {
		res.status(500).json({ error: error.message });
	}
});

app.use("/admin", express.static("public"));
app.listen(port, () => console.log(`🚀 Server running on port ${port}`));
